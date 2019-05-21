ecfo off
for /r input %%i in (*.txt) do (
	echo %%i
	rem запускаем тест
	echo on 
	echo ---------------------------------------------------	
	echo -------------start test: %%~ni---------------------
	echo off
	"..\AntiClosure.dll" %%i %%~pi\..\output\%%~ni.txt
	echo on
	echo ---------------------------------------------------	
	echo "|||||||||||||||||||||||||||||||||||||||||||||||||||"
	echo off
)

echo !!!!!all tests complete!!!!!


@echo off

call :sleep 20

:sleep
setlocal
    if %time:~5,2% EQU :0 (set /a ftime=%time:~7,-3%+%1 ) else ( set /a ftime=%time:~6,-3%+%1 )
    if %ftime% GEQ 60 set /a ftime-=60
    :loop
     if %time:~5,2% EQU :0 (set /a ctime=%time:~7,-3% ) else ( set /a ctime=%time:~6,-3% )
        if /i %ftime% NEQ %ctime% goto :loop
endlocal
exit /b 0