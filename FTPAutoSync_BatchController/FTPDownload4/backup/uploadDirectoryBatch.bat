TITLE MyFirstBatchFile
echo OFF
echo Do you know what this is? It's my first batch file.
set sourcePath=%1
set destinationPath=%2
set folderName=%3
echo Source Path: %sourcePath%
echo Destination Path: %destinationPath%
echo Folder Name: %folderName%
echo open 10.220.20.25>ftp.txt
echo anonymous>>ftp.txt
echo abc>>ftp.txt
echo cd ./%destinationPath%>>ftp.txt
echo mkdir %folderName%>>ftp.txt
echo cd ./%folderName%>>ftp.txt
echo lcd %sourcePath%>>ftp.txt
echo prompt>>ftp.txt
echo mput *>>ftp.txt
echo bye>>ftp.txt
ftp -s:ftp.txt
del ftp.txt