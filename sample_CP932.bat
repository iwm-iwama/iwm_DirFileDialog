@echo off
chcp 932 >NUL
cls

set CMD=iwm_DirFileDialog.exe

echo.

:R10
	echo �t�H���_�I��
	for /f "usebackq delims=" %%s in (`"%CMD% -t=d"`) do echo %%s
:R19

echo.

:R20
	echo �t�@�C���I��
	for /f "usebackq delims=" %%s in (`"%CMD% -t=m"`) do echo %%s
:R29

echo.

pause
exit
