
'
' VBScript example using the Windows scripting engine
'

Class Script
    Dim driver

    Sub Class_Initialize
        Set Waiter = CreateObject("Selenium.Waiter")
        Set Assert = CreateObject("Selenium.Assert")
        Set driver = CreateObject("Selenium.FirefoxDriver")
        
        'open the browser and the page
        driver.Get "https://www.google.co.uk"
        'type into the search input box
        driver.FindElementByName("q").SendKeys "Eiffel tower" & driver.Keys.Return
        'waits for the title
        While Waiter.Not(Instr(driver.Title, "Eiffel tower")): Wend
        'waits for the user to click OK
        WScript.Echo "Click OK to quit the browser"
    End Sub

    Sub Class_Terminate
        driver.Quit    'Stop the browser
    End Sub
End Class

Set s = New Script
