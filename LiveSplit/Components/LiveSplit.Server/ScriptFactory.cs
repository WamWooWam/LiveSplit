using System;

namespace LiveSplit
{
    public class ScriptFactory
    {
        public static readonly string[] AllScripts = { "JavaScript", "C#" };

        public static IScript Create(string language, string code)
        {
            var lowerLanguage = language.ToLower();

            if (lowerLanguage == "javascript" || lowerLanguage == "js")
                throw new NotImplementedException("JavaScript has been disabled for security reasons.");
            if (lowerLanguage == "c#" || lowerLanguage == "cs")
                return new CSharpScript(code);

            throw new ArgumentException("The language does not exist", "language");
        }
    }
}
