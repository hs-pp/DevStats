using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DevStatsSystem.Editor.Core
{
    internal struct Argument
    {
        public string Option;
        public string Value;

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Value))
            {
                return $" {Option}";
            }
            else
            {
                return $" {Option} \"{Value}\"";
            }
        }
    }
    
    internal class WakatimeCliArguments
    {
        private List<Argument> m_args = new();

        public WakatimeCliArguments AddKey()
        {
            return AddArgument("--key", DevStatsSettings.Get().APIKey);
        }
        public WakatimeCliArguments AddFile(string file) => AddArgument("--entity", file);
        public WakatimeCliArguments AddTimestamp(decimal timestamp) => AddArgument("--time", timestamp.ToString(CultureInfo.InvariantCulture));
        public WakatimeCliArguments AddProject(string project) => AddArgument("--project", project);
        public WakatimeCliArguments AddPlugin() => AddArgument("--plugin", "DevStats");
        public WakatimeCliArguments AddEntityType(string entityType) => AddArgument("--entity-type", entityType);
        public WakatimeCliArguments AddExtraHeartbeats() => AddArgument("--extra-heartbeats");
        public WakatimeCliArguments AddCategory(string category) => AddArgument("--category", category);
        public WakatimeCliArguments AddIsWrite(bool isWrite)
        {
            if (isWrite)
            {
                return AddArgument("--write");
            }
            return this;
        }
        
        /// <summary>
        /// Functional programming lfg
        /// </summary>
        private WakatimeCliArguments AddArgument(string option, string value = null)
        {
            TryAddArgument(new Argument()
            {
                Option = option,
                Value = value,
            });
            return this;
        }

        private void TryAddArgument(Argument newArg)
        {
            if (string.IsNullOrEmpty(newArg.Option))
            {
                DevStats.LogError("Tried to add an arg with no Option!");
                return;
            }
            
            if (m_args.Exists(x => x.Option == newArg.Option))
            {
                DevStats.LogError($"Tried to add an existing arg! {newArg.Option}");
                return;
            }
            
            m_args.Add(newArg);
        }

        public string ToArgs(bool isSanitized = true)
        {
            StringBuilder strBuilder = new();
            foreach (Argument arg in m_args)
            {
                if (isSanitized && arg.Option == "--key") // special case where we don't want to expose key.
                {
                    strBuilder.Append($" --key XXXXXX");
                }
                else
                {
                    strBuilder.Append(arg.ToString());
                }
            }
            
            return strBuilder.ToString();
        }

        public override string ToString()
        {
            return ToArgs();
        }

        public static WakatimeCliArguments Help() => new WakatimeCliArguments().AddArgument("--help");
        public static WakatimeCliArguments Version() => new WakatimeCliArguments().AddArgument("--version");
    }
}