require(tcltk)
install.packages('snowfall')
tkmessageBox(message="Now select the OpenMx binary when prompted")
filename <- tclvalue(tkgetOpenFile())
install.packages(filename, repos=NULL)