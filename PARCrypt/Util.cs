namespace PARCrypt
{
    internal class Util
    {
        internal static byte[] GetEmbeddedKeyFile(string name)
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            string resourceName = asm.GetManifestResourceNames()
                .SingleOrDefault(str => str.EndsWith($"{name}.key"));
            if (resourceName == null) return null;
            using (Stream resFilestream = asm.GetManifestResourceStream(resourceName))
            {
                if (resFilestream == null) return null;
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }
    }
}
