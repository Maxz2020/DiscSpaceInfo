namespace dsi.console
{
    internal class CmdLineArgs
    {
        public string Action { get; set; }

        public List<string> Params { get; set; }

        public List<string> Options { get; set; }

        public CmdLineArgs(string[] args)
        {
            Options = new List<string>(args.Length);
            Params = new List<string>(args.Length);

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--"))
                {
                    Options.Add(args[i]);
                }
                else if (args[i].StartsWith("-"))
                {
                    Action = args[i];
                }
                else
                {
                    args[i] = args[i].Replace('"', '\\');

                    Params.Add(args[i]);
                }
            }

            if (Action == null)
            {
                throw new ArgumentException("Не указан обязательный параметр: \"действие\"");
            }
        }
    }
}
