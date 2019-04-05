using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ExtendLogging
{
    //Copied From https://github.com/Bililive/BililiveRecorder/blob/dev/BililiveRecorder.Core/Utils.cs#L72
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DoNotCopyProperty : Attribute { }

    public class Settings : INotifyPropertyChanged
    {
        protected readonly string FilePath;
        public Settings(string filePath)
        {
            FilePath = filePath;
        }
        public virtual void SaveConfig()
        {
            JObject j = JObject.FromObject(this);
            File.WriteAllText(FilePath, j.ToString());
        }
        public virtual void LoadConfig()
        {
            if (File.Exists(FilePath))
            {
                Type configType = this.GetType();
                object configInstance = JsonConvert.DeserializeObject(File.ReadAllText(FilePath), configType);
                foreach (PropertyInfo property in configType.GetProperties())
                {
                    if (!Attribute.IsDefined(property, typeof(DoNotCopyProperty)))
                    {
                        property.SetValue(this, property.GetValue(configInstance));
                    }
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public sealed class PluginSettings : Settings
    {
        private bool _Enabled;
        public bool Enabled { get => _Enabled; set { if (_Enabled != value) { _Enabled = value; OnPropertyChanged(); } } }
        private bool _LogLevel = true;
        public bool LogLevel { get => _LogLevel; set { if (_LogLevel != value) { _LogLevel = value; OnPropertyChanged(); } } }
        private bool _LogMedal = true;
        public bool LogMedal { get => _LogMedal; set { if (_LogMedal != value) { _LogMedal = value; OnPropertyChanged(); } } }
        private bool _LogTitle = true;
        public bool LogTitle { get => _LogTitle; set { if (_LogTitle != value) { _LogTitle = value; OnPropertyChanged(); } } }
        private bool _LogExternInfo = true;
        public bool LogExternInfo { get => _LogExternInfo; set { if (_LogExternInfo != value) { _LogExternInfo = value; OnPropertyChanged(); } } }
        private bool _HideGifts = false;
        public bool HideGifts { get => _HideGifts; set { if (_HideGifts != value) { _HideGifts = value; OnPropertyChanged(); } } }
        private bool _EnableShieldLevel = false;
        public bool EnableShieldLevel { get => _EnableShieldLevel; set { if (_EnableShieldLevel != value) { _EnableShieldLevel = value; OnPropertyChanged(); } } }
        private int _ShieldLevel = 0;
        public int ShieldLevel { get => _ShieldLevel; set { if (_ShieldLevel != value) { _ShieldLevel = value; OnPropertyChanged(); } } }
        public PluginSettings(string filePath) : base(filePath)
        {

        }
        protected override void OnPropertyChanged([CallerMemberName]string name = null)
        {
            if (!string.IsNullOrEmpty(FilePath))
            {
                SaveConfig();
            }
            base.OnPropertyChanged(name);
        }
    }
}
