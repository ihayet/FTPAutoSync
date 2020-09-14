TITLE MyFirstBatchFile
echo OFF
echo Do you know what this is? It's my first batch file.
set folderName=%1
set fileName=%2
set destinationPath=%3
set ftpAddress=%4
echo Folder Name: %folderName%
echo File Name: %fileName%
echo Destination Path: %destinationPath%
echo FTP Address: %ftpAddress%
copy /y NUL ftpDownload.txt
echo open %ftpAddress%>ftpDownload.txt
echo anonymous>>ftpDownload.txt
echo abc>>ftpDownload.txt
echo cd ./%folderName%>>ftpDownload.txt
echo dir>>ftpDownload.txt
echo lcd %destinationPath%>>ftpDownload.txt
echo hash>>ftpDownload.txt
echo recv %fileName%>>ftpDownload.txt
echo bye>>ftpDownload.txt
@ftp -s:ftpDownload.txt
del ftpDownload.txt