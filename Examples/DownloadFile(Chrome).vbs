
' ---------------------------------------------------------------------------------------
' Set the browser to automatically download files without popup to a target folder
' ---------------------------------------------------------------------------------------

Class Script
	Dim driver
	
	Sub Class_Initialize
		folder = Replace(WScript.ScriptFullName, WScript.ScriptName,"") & "chrome_download"
		
		Set driver = CreateObject("Selenium.ChromeDriver")
        driver.SetPreference "download.default_directory", folder 
        driver.SetPreference "download.directory_upgrade", true
        driver.SetPreference "download.prompt_for_download", false
        driver.SetPreference "plugins.plugins_disabled", Array("Chrome PDF Viewer")
        
		driver.Get "http://static.mozilla.com/moco/en-US/pdf/mozilla_privacypolicy.pdf"
	End Sub

	Sub Class_Terminate
        WScript.Echo "Click OK to close the browser"
		driver.Quit
	End Sub
End Class

Set s = New Script

