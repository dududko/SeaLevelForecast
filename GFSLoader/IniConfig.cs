using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FloodForecasting
{
    public class IniConfig
    {
        private readonly Dictionary<string, Dictionary<string, string>> _sections;

        public IniConfig() : this((IniConfig)null) { }

        public IniConfig(IniConfig copyFrom)
        {
            _sections = copyFrom == null
              ? new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
              : copyFrom._sections.ToDictionary(kv => kv.Key,
                kv => new Dictionary<string, string>(kv.Value, StringComparer.OrdinalIgnoreCase),
                StringComparer.OrdinalIgnoreCase);
        }

        public IniConfig(string filePath)
            : this()
        {
            try
            {
                using (StreamReader reader = File.OpenText(filePath))
                {
                    string line;
                    Dictionary<string, string> currentSection = null;

                    int currentLineNumber = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        ++currentLineNumber;
                        line = line.Trim();

                        if (line.StartsWith("["))
                        {
                            if (!line.EndsWith("]"))
                            {
                                throw new ExceptionWrapper(string.Format("Unexpected string format at line {0}: {1}.",
                                  currentLineNumber, line));
                            }

                            string currentSectionName = line.Substring(1, line.Length - 2).Trim();

                            if (_sections.TryGetValue(currentSectionName, out currentSection))
                                continue;

                            currentSection = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                            _sections.Add(currentSectionName, currentSection);
                        }
                        else if (!line.StartsWith("//") && !line.Equals(string.Empty))
                        {
                            if (currentSection == null && !_sections.TryGetValue(string.Empty, out currentSection))
                            {
                                currentSection = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                                _sections.Add(string.Empty, currentSection);
                            }

                            int delimIdx = line.IndexOf('=');

                            if (delimIdx <= 0)
                            {
                                throw new ExceptionWrapper(string.Format("Unexpected string format at line {0}: {1}.",
                                  currentLineNumber, line));
                            }

                            string paramName = line.Substring(0, delimIdx).TrimEnd();

                            if (currentSection.ContainsKey(paramName))
                            {
                                throw new ExceptionWrapper(string.Format("Parameter duplicate at line {0}: {1}.", currentLineNumber,
                                  line));
                            }

                            currentSection.Add(paramName, line.Substring(delimIdx + 1).TrimStart());
                        }
                    }
                }
            }
            catch (ExceptionWrapper e)
            {
                throw new ExceptionWrapper(string.Format("Unexpected configuration file format: {0}.", filePath), e);
            }
            catch (Exception e)
            {
                throw new ExceptionWrapper(string.Format("Failed to read configuration file: {0}.", filePath), e);
            }
        }

        public string this[string sectionName,
          string paramName]
        {
            get
            {
                try
                {
                    if (sectionName == null || paramName == null)
                        throw new ArgumentNullException();

                    string result;
                    if (!TryGetValue(sectionName, paramName, out result))
                        throw new ArgumentOutOfRangeException();

                    return result;
                }
                catch (Exception e)
                {
                    throw new ExceptionWrapper(
                      string.Format("Failed to get parameter [{0}]{1}.", sectionName ?? "NULL", paramName ?? "NULL"), e);
                }
            }

            set
            {
                try
                {
                    if (sectionName == null || paramName == null)
                        throw new ArgumentNullException();

                    sectionName = sectionName.Trim();
                    paramName = paramName.Trim();

                    Dictionary<string, string> section;
                    if (!_sections.TryGetValue(sectionName, out section))
                    {
                        section = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        _sections.Add(sectionName, section);
                    }

                    section[paramName] = value == null ? string.Empty : value.Trim();
                }
                catch (Exception e)
                {
                    throw new ExceptionWrapper(
                      string.Format("Failed to set parameter [{0}]{1}={2}.", sectionName ?? "NULL", paramName ?? "NULL",
                        value ?? string.Empty), e);
                }
            }
        }

        public bool TryGetValue(
          string sectionName,
          string paramName,
          out string value)
        {
            value = null;

            try
            {
                Dictionary<string, string> section;
                return _sections.TryGetValue(sectionName, out section) && section.TryGetValue(paramName, out value);
            }
            catch (Exception e)
            {
                throw new ExceptionWrapper(
                  string.Format("Failed to get parameter [{0}]{1}.", sectionName ?? "NULL", paramName ?? "NULL"), e);
            }
        }

        public bool Contains(
          string sectionName,
          string paramName)
        {
            try
            {
                Dictionary<string, string> section;
                return _sections.TryGetValue(sectionName, out section) && section.ContainsKey(paramName);
            }
            catch (Exception e)
            {
                throw new ExceptionWrapper(
                  string.Format("Failed to check if parameter [{0}]{1} is contained.", sectionName ?? "NULL",
                    paramName ?? "NULL"), e);
            }
        }

        public void SaveTo(Stream stream)
        {
            try
            {
                using (var writer = new StreamWriter(stream))
                {
                    writer.Write(string.Join(string.Concat(Environment.NewLine, Environment.NewLine),
                      _sections.Select(
                        section =>
                          string.Concat('[', section.Key, ']', Environment.NewLine,
                            string.Join(Environment.NewLine,
                              section.Value.Select(parameter => string.Concat(parameter.Key, " = ", parameter.Value)))))));
                }
            }
            catch (Exception e)
            {
                throw new ExceptionWrapper("Failed to serialize configuration.", e);
            }
        }

        public void Merge(
          IniConfig config,
          bool overwrite)
        {
            try
            {
                foreach (var section in config._sections)
                {
                    Dictionary<string, string> thisSection;
                    if (!_sections.TryGetValue(section.Key, out thisSection))
                    {
                        thisSection = new Dictionary<string, string>(section.Value.Count, StringComparer.OrdinalIgnoreCase);
                        _sections.Add(section.Key, thisSection);
                    }

                    foreach (var param in section.Value.Where(param => !thisSection.ContainsKey(param.Key) || overwrite))
                        thisSection[param.Key] = param.Value;
                }
            }
            catch (Exception e)
            {
                throw new ExceptionWrapper("Failed to merge configurations.", e);
            }
        }

        public bool RemoveSection(string sectionName)
        {
            try
            {
                return _sections.Remove(sectionName);
            }
            catch (Exception e)
            {
                throw new ExceptionWrapper(string.Format("Failed to remove section [{0}].", sectionName ?? "NULL"), e);
            }
        }

        public bool RemoveParameter(
          string sectionName,
          string paramName)
        {
            try
            {
                Dictionary<string, string> section;
                return _sections.TryGetValue(sectionName, out section) && section.Remove(paramName);
            }
            catch (Exception e)
            {
                throw new ExceptionWrapper(
                  string.Format("Failed to remove parameter [{0}]{1}.", sectionName ?? "NULL", paramName ?? "NULL"), e);
            }
        }
    }
}