
' ---------------------------------------------------------------------------------------
' Enabling NTLM Authentication (Single Sign-On) in Firefox
' ---------------------------------------------------------------------------------------

Class Script
	Dim driver

	Sub Class_Initialize
		Set driver = CreateObject("Selenium.FirefoxDriver")        
		driver.Get "http://test:test@browserspy.dk/password-ok.php"
        
	End Sub

	Sub Class_Terminate
        WScript.Echo "Click OK to quit the browser"
        on error resume next
		driver.Quit
	End Sub
End Class

Set s = New Script
