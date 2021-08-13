@echo off

SET MYFILE="AutobiographicalStorage\episodicMemory.txt"
IF EXIST %MYFILE% DEL /F %MYFILE%
copy AutobiographicalStorage\backupepisodicMemoryWordnet.txt AutobiographicalStorage\episodicMemory.txt

CLS 2>AutobiographicalStorage\historic.txt
CLS 2>AutobiographicalStorage\smallTalksUsed.txt
CLS 2>AutobiographicalStorage\chatLog.txt

SET "sourcedir=AutobiographicalStorage\Images"
SET "file1=Arthur.png"
SET "file2=Bella.png"
FOR %%a IN ("%sourcedir%\*") DO IF /i NOT "%%~nxa"=="%file1%" IF /i NOT "%%~nxa"=="%file2%" DEL "%%a"

echo 10029> nextId.txt
echo 10016>> nextId.txt

SET PYTHONDIR="Assets\Python"
IF EXIST %PYTHONDIR%\facefile.npy DEL /F %PYTHONDIR%\facefile.npy
IF EXIST %PYTHONDIR%\namefile.npy DEL /F %PYTHONDIR%\namefile.npy
RD /S /Q %PYTHONDIR%\Data
MD %PYTHONDIR%\Data

PAUSE