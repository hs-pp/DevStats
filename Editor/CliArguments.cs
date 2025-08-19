using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace DevStats.Editor
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
    
    internal class CliArguments
    {
        private List<Argument> m_args = new();

        public CliArguments AddKey()
        {
            return AddArgument("--key", DevStatsSettings.Get().APIKey);
        }
        public CliArguments AddFile(string file) => AddArgument("--entity", file);
        public CliArguments AddTimestamp(decimal timestamp) => AddArgument("--time", timestamp.ToString(CultureInfo.InvariantCulture));
        public CliArguments AddIsWrite(bool isWrite) => AddArgument("--write", isWrite.ToString().ToLower());
        public CliArguments AddProject(string project) => AddArgument("--project", project);
        public CliArguments AddPlugin() => AddArgument("--plugin", "DevStats");
        public CliArguments AddEntityType(string entityType) => AddArgument("--entity-type", entityType);
        public CliArguments AddExtraHeartbeats() => AddArgument("--extra-heartbeats");
        public CliArguments AddCategory(string category)
        {
            if (!string.IsNullOrEmpty(category))
            {
                return AddArgument("--category", category);
            }

            return this;
        }

        /// <summary>
        /// Functional programming lfg
        /// </summary>
        private CliArguments AddArgument(string option, string value = null)
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
                Debug.LogError($"Error! Tried to add an arg with no Option!");
                return;
            }
            
            if (m_args.Exists(x => x.Option == newArg.Option))
            {
                Debug.LogError($"Error! Tried to add an existing arg! {newArg.Option}");
                return;
            }
            
            m_args.Add(newArg);
        }

        public string ToArgs(bool isSanitized = false)
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
            return ToArgs(true);
        }

        public static CliArguments Help() => new CliArguments().AddArgument("--help");
        public static CliArguments Version() => new CliArguments().AddArgument("--version");
    }
}