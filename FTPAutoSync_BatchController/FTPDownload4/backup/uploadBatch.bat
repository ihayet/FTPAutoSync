TITLE MyFirstBatchFile
echo OFF
echo Do you know what this is? It's my first batch file.
set sourcePath=%1
set destinationPath=%2
echo Source Path: %sourcePath%
echo Destination Path: %destinationPath%
echo open 10.220.20.25>ftp.txt
echo anonymous>>ftp.txt
echo abc>>ftp.txt
echo cd ./%destinationPath%>>ftp.txt
echo hash>>ftp.txt
echo put %sourcePath%>>ftp.txt
echo bye>>ftp.txt
ftp -s:ftp.txt
del ftp.txt