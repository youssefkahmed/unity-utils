using UnityEditor;
using UnityEditor.Compilation;

namespace Playmaykr.Utils.Hotkeys.Editor
{
    public static class CompileProject
    {
        [MenuItem("File/Compile _F5")]
        private static void Compile()
        {
            CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
        }
    }
}