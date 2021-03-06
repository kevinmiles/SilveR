using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SilveR.Helpers;
using SilveR.Models;
using SilveR.StatsModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SilveR.Services
{
    public interface IRProcessorService
    {
        Task Execute(string analysisGuid);
    }

    public class RProcessorService : IRProcessorService
    {
        private readonly IServiceProvider services;

        public RProcessorService(IServiceProvider services)
        {
            this.services = services;
        }

        public async Task Execute(string analysisGuid)
        {
            using (IServiceScope scope = services.CreateScope())
            {
                ISilveRRepository silveRRepository = scope.ServiceProvider.GetRequiredService<ISilveRRepository>();
                AppSettings appSettings = scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>().Value;

                //declared here as used in exception handler
                string workingDir = Path.GetTempPath();
                string theArguments = null;
                string rscriptPath = null;

                try
                {
                    Stopwatch sw = Stopwatch.StartNew();

                    //get analysis
                    Analysis analysis = await silveRRepository.GetAnalysisComplete(analysisGuid);

                    //save the useroptions to the working dir
                    UserOption userOptions = await silveRRepository.GetUserOptions();
                    File.WriteAllLines(Path.Combine(workingDir, "UserOptions.txt"), userOptions.GetOptionLines());

                    //combine script files into analysisGuid.R
                    string scriptFileName = Path.Combine(workingDir, analysisGuid + ".R");
                    string[] commonFunctionsContents = File.ReadAllLines(Path.Combine(Startup.ContentRootPath, "Scripts", "Common_Functions.R"));
                    string[] mainScriptContents = File.ReadAllLines(Path.Combine(Startup.ContentRootPath, "Scripts", analysis.Script.ScriptFileName + ".R"));

                    File.WriteAllLines(scriptFileName, commonFunctionsContents);
                    File.AppendAllLines(scriptFileName, mainScriptContents);

                    //load the analysis entity into the model so that arguments can be extracted
                    AnalysisModelBase analysisModel = AnalysisFactory.CreateAnalysisModel(analysis);
                    analysisModel.LoadArguments(analysis.Arguments);

                    //csvfilename is built from the analysis guid and is also used in R to name the output at this time
                    string csvFileName = Path.Combine(workingDir, analysisGuid + ".csv");

                    AnalysisDataModelBase analysisDataModelBase = analysisModel as AnalysisDataModelBase;
                    if (analysisDataModelBase != null) //then has data component
                    {
                        string[] csvData = analysisDataModelBase.ExportData();
                        File.WriteAllLines(csvFileName, csvData);
                    }

                    //setup the r process (way of calling rscript.exe is slightly different for each OS)
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        if (!String.IsNullOrEmpty(appSettings.CustomRScriptLocation))
                        {
                            rscriptPath = appSettings.CustomRScriptLocation;
                        }
                        else
                        {
                            rscriptPath = Path.Combine(Startup.ContentRootPath, "R-3.5.1", "bin", "Rscript.exe");
                        }
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        if (!String.IsNullOrEmpty(appSettings.CustomRScriptLocation))
                        {
                            rscriptPath = appSettings.CustomRScriptLocation;
                        }
                        else
                        {
                            rscriptPath = "Rscript";
                            //rscriptPath = Path.Combine(Startup.ContentRootPath, "R-3.5.1", "bin", "Rscript");
                        }
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        rscriptPath = "Rscript";
                    }

                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = rscriptPath;
                    psi.WorkingDirectory = workingDir;

                    theArguments = analysisModel.GetCommandLineArguments();
                    psi.Arguments = FormatPreArgument(scriptFileName) + " --vanilla --args " + FormatPreArgument(csvFileName) + " " + theArguments;

                    //Configure some options for the R process
                    psi.UseShellExecute = false;
                    psi.CreateNoWindow = true;
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;

                    //start rscript.exe
                    Process R = Process.Start(psi);

                    //create a stringbuilder to hold the output, and get the output line by line until R process finishes
                    StringBuilder output = new StringBuilder();

                    bool completedOK = R.WaitForExit(60 * 1000);

                    //if R didn't complete in time then add message and kill it
                    if (!completedOK)
                    {
                        output.AppendLine("WARNING! The R process timed out before the script could complete");
                        output.AppendLine();

                        R.Kill();
                    }

                    //need to make sure that we have got all the output so do a readtoend here
                    output.AppendLine(R.StandardOutput.ReadToEnd());
                    output.AppendLine();

                    //output the errors from R
                    string errorsFromR = R.StandardError.ReadToEnd().Trim();

                    R.Close();
                    R.Dispose();

                    if (!String.IsNullOrEmpty(errorsFromR))
                    {
                        output.AppendLine();
                        output.Append(errorsFromR);
                        output.AppendLine();
                    }

                    TimeSpan timeElapsed = sw.Elapsed;
                    output.AppendLine();
                    output.AppendLine("Analysis by the R Processor took " + Math.Round(timeElapsed.TotalSeconds, 2) + " seconds.");

                    analysis.RProcessOutput = output.ToString().Trim();

                    //assemble the entire path and file to the html output
                    string htmlFile = Path.Combine(workingDir, analysisGuid + ".html");

                    if (File.Exists(htmlFile)) //wont exist if there is an error!
                    {
                        //get the output files that match the guid
                        List<string> resultsFiles = new List<string>();
                        string[] outputFiles = Directory.GetFiles(workingDir, analysis.AnalysisGuid + "*");
                        foreach (string file in outputFiles.Where(x => !x.EndsWith(".R"))) //go through all output (except the R input file)
                        {
                            FileInfo fileInfo = new FileInfo(file);
                            if (fileInfo.Extension == ".html" || fileInfo.Extension == ".jpg") //main output so add to results
                            {
                                resultsFiles.Add(file);
                            }
                            else if (fileInfo.Name != analysis.AnalysisGuid + ".csv" && fileInfo.Extension == ".csv") //is not the input csv and is a dataset to be imported into datasets
                            {
                                DataTable dataTable = CSVConverter.CSVDataToDataTable(fileInfo.OpenRead(), System.Threading.Thread.CurrentThread.CurrentCulture);

                                string datasetName = fileInfo.Name.Replace(analysis.AnalysisGuid, String.Empty).TrimStart('-');
                                await SaveDatasetToDatabase(silveRRepository, datasetName, dataTable);
                            }
                            //else
                            //    throw new InvalidOperationException("Unexpected file type in temp folder!");
                        }

                        string inlineHtml = InlineHtmlCreator.CreateInlineHtml(resultsFiles);

                        analysis.HtmlOutput = inlineHtml;
                    }

                    await silveRRepository.UpdateAnalysis(analysis);

#if !DEBUG
                    string[] filesToClean = Directory.GetFiles(workingDir, analysis.AnalysisGuid + "*");
                    foreach (string file in filesToClean)
                        File.Delete(file);
#endif
                }
                catch (Exception ex)
                {
                    try
                    {
                        Analysis analysis = await silveRRepository.GetAnalysis(analysisGuid);

                        string message = "ContentRoot=" + Startup.ContentRootPath + Environment.NewLine + Environment.NewLine;
                        message = message + "TempFolder=" + workingDir + Environment.NewLine + Environment.NewLine;
                        message = message + "Arguments=" + theArguments + Environment.NewLine + Environment.NewLine;
                        message = message + "Rscript=" + rscriptPath + Environment.NewLine + Environment.NewLine;

                        message = message + ex.Message;

                        if (ex.InnerException != null)
                        {
                            message = message + Environment.NewLine + "Inner Exception: " + ex.InnerException.Message;
                        }

                        analysis.RProcessOutput = message;
                        await silveRRepository.UpdateAnalysis(analysis);
                    }
                    catch { }

                    throw ex;
                }

            }
        }

        private async Task SaveDatasetToDatabase(ISilveRRepository silveRRepository, string fileName, DataTable dataTable)
        {
            //get last version no based on existing dataset names
            int lastVersionNo = await silveRRepository.GetLastVersionNumberForDataset(fileName);
            Dataset dataset = dataTable.GetDataset(fileName, lastVersionNo);

            await silveRRepository.CreateDataset(dataset);
        }

        private string FormatPreArgument(string str)
        {
            return "\"" + str + "\"";
        }
    }
}