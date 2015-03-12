REM Install the AppDynamics agent on Windows Azure
SETLOCAL EnableExtensions
SET CONFIGUPDATE=true
IF {%INTERNAL_APPDYNAMICS_AGENT_INSTALL_REBOOT%}=={true} (
	SETX INTERNAL_APPDYNAMICS_AGENT_INSTALL_REBOOT ""
	GOTO :END
)

REM Bypass the installation if this is emulated environment
IF {%EMULATED%}=={true} GOTO :END

REM Do nothing if other profiler is installed
IF NOT "%COR_PROFILER%"=="" (IF NOT "%COR_PROFILER%"=="AppDynamics.AgentProfiler" GOTO :END)

REM Uninstall if pre 3.8.0 .NET Agent already installed
SET PRODUCTCODE={0C633F51-09FE-4AE4-A25F-F6CD167CC46E}
SET REGKEY=HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\%PRODUCTCODE%
REG QUERY %REGKEY%
IF %ERRORLEVEL%==0 (
	start /wait msiexec /x %PRODUCTCODE% /quiet /log d:\aduninstall.log
	IF NOT %ERRORLEVEL%==0 SHUTDOWN /r /f /c "Reboot after uninstalling the AppDynamics .NET Agent"
)

IF NOT EXIST AppDynamics\dotNetAgentSetup64.msi GOTO :END

REM See if current agent version is installed
SET REGKEYVER="HKEY_LOCAL_MACHINE\Software\AppDynamics\dotNet Agent"
SET REGVALNAMEVER=Version
SET INSTALLVER=3.9.6.5
SET REGVALUEVER=""
FOR /F "tokens=2*" %%A IN ('REG QUERY %REGKEYVER% /v %REGVALNAMEVER%') DO SET REGVALUEVER=%%B

IF %REGVALUEVER%==%INSTALLVER% GOTO :END

SET ControllerHost=%1
SET ControllerPort=%2 
SET AccountName=%3 
SET AccountAccessKey=%4 
SET ControllerApplication=%5
SET ControllerSSLEnabled=%6

IF {%1}=={} (ECHO Syntax error: Missing AD_Agent_ControllerHost parameter >>d:\adInstall.log) & (GOTO :END)
IF {%2}=={} (ECHO Syntax error: Missing AD_Agent_ControllerPort parameter >>d:\adInstall.log) & (GOTO :END)
IF {%3}=={} (ECHO Syntax error: Missing AD_Agent_AccountName parameter >>d:\adInstall.log) & (GOTO :END)
IF {%4}=={} (ECHO Syntax error: Missing AD_Agent_AccountAccessKey parameter >>d:\adInstall.log) & (GOTO :END)
IF {%5}=={} (SET ControllerApplication=Default)
IF {%6}=={} (SET ControllerSSLEnabled=false)

REM Install the agent
start /wait msiexec /i AppDynamics\dotNetAgentSetup64.msi AD_AGENT_ENVIRONMENT=Azure AD_AZUREROLENAME=%RoleName% AD_AZUREROLEINSTANCEID=%RoleInstanceID% AD_AGENT_CONTROLLERHOST=%ControllerHost% AD_AGENT_CONTROLLERPORT=%ControllerPort% AD_AGENT_ACCOUNTNAME=%AccountName% AD_AGENT_ACCOUNTACCESSKEY=%AccountAccessKey% AD_AGENT_CONTROLLERAPPLICATION=%ControllerApplication% AD_AGENT_CONTROLLERSSLENABLED=%ControllerSSLEnabled% /qn /l*v d:\adInstall.log 
IF %ERRORLEVEL%==0 (
	SETX INTERNAL_APPDYNAMICS_AGENT_INSTALL_REBOOT "true"
	REM Reboot the machine after installation in order to restart role CLR and attach AppDynamics Agent to it
	SHUTDOWN /r /f /c "Reboot after installing the AppDynamics .NET Agent"
)

:END
EXIT /B %ERRORLEVEL%
