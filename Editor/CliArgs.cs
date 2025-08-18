using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DevStats.Editor
{
    public struct Argument
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
    
    public class CliArgs
    {
        private List<Argument> m_args = new();

        public CliArgs AddKey(string key) => AddArgument("--key", key);
        public CliArgs AddFile(string file) => AddArgument("--file", file);

        /// <summary>
        /// Functional programming lfg
        /// </summary>
        private CliArgs AddArgument(string option, string value = null)
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

        public static CliArgs Help() => new CliArgs().AddArgument("--help");
        public static CliArgs Version() => new CliArgs().AddArgument("--version");
    }
}