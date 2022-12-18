namespace ChatGPT.Net.DTO;

public class StealthConfig
{
    public bool ChromeFp { get; set; } = true;
    public bool ChromeGlobal { get; set; } = true;
    public bool ChromeTouch { get; set; } = true;
    public bool NavigatorPermissions { get; set; } = true;
    public bool NavigatorWebdriver { get; set; } = true;
    public bool ChromeRuntime { get; set; } = true;
    public bool ChromePlugin { get; set; } = true;
    public bool MainFunction { get; set; } = true;
    public IEnumerable<string> EnabledScripts
    {
        get
        {
            if (ChromeFp)
            {
                yield return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "JsScripts", "canvas.fingerprinting.js"));
            }
            if (ChromeGlobal)
            {
                yield return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "JsScripts", "chrome.global.js"));
            }
            if (ChromeTouch)
            {
                yield return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "JsScripts", "emulate.touch.js"));
            }
            if (NavigatorPermissions)
            {
                yield return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "JsScripts", "navigator.permissions.js"));
            }
            if (NavigatorWebdriver)
            {
                yield return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "JsScripts", "navigator.webdriver.js"));
            }
            if (ChromePlugin)
            {
                yield return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "JsScripts", "chrome.runtime.js"));
            }
            if (ChromeRuntime)
            {
                yield return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "JsScripts", "chrome.plugin.js"));
            }
            if (MainFunction)
            {
                yield return File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "JsScripts", "mainFunctions.js"));
            }
        }
    }
}