﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tibialyzer {
    public enum StatusType { Health, Mana, Experience, ExpPerHour };

    public partial class StatusBar : BaseHUD {
        private StatusType statusType;
        private bool displayText;

        public StatusBar(StatusType statusType) {
            this.statusType = statusType;
            InitializeComponent();
            
            BackColor = StyleManager.BlendTransparencyKey;
            TransparencyKey = StyleManager.BlendTransparencyKey;

            displayText = SettingsManager.getSettingBool(GetHUD() + "DisplayText");
            double opacity = SettingsManager.getSettingDouble(GetHUD() + "Opacity");
            opacity = Math.Min(1, Math.Max(0, opacity));
            this.Opacity = opacity;
        }

        ~StatusBar() {
            if (timer != null) {
                timer.Stop();
                timer.Dispose();
            }
        }

        public override void LoadHUD() {
            double fontSize = SettingsManager.getSettingDouble(GetHUD() + "FontSize");
            fontSize = fontSize < 0 ? 20 : fontSize;
            this.healthBarLabel.Font = new System.Drawing.Font("Verdana", (float)fontSize, System.Drawing.FontStyle.Bold);
            this.RefreshHUD(100, 100);
            this.Load += StatusBar_Load;
        }
        
        private SafeTimer timer;
        private void StatusBar_Load(object sender, EventArgs e) {
            timer = new SafeTimer(10, Timer_Tick);
            timer.Start();
        }
        
        private void Timer_Tick() {
            RefreshHealth();
        }

        public static long GetExperience(long lvl) {
            return (50 * lvl * lvl * lvl - 150 * lvl * lvl + 400 * lvl) / 3;
        }

        public void RefreshHUD(long value, long max) {
            double percentage = ((double) value) / ((double) max);
            if (displayText) {
                if (statusType == StatusType.Experience) {
                    healthBarLabel.Text = String.Format("Lvl {0}: {1}%", MemoryReader.Level, (int)(percentage * 100));
                } else {
                    healthBarLabel.Text = String.Format("{0}/{1}", value, max);
                }
            } else {
                healthBarLabel.Text = "";
            }
            healthBarLabel.percentage = percentage;
            healthBarLabel.Size = this.Size;
            if (statusType == StatusType.Health) {
                healthBarLabel.BackColor = StyleManager.GetHealthColor(percentage);
            } else if (statusType == StatusType.Mana) {
                healthBarLabel.BackColor = StyleManager.ManaColor;
            } else if (statusType == StatusType.Experience) {
                healthBarLabel.BackColor = StyleManager.ExperienceColor;
            }
        }

        public void RefreshHealth() {
            long life = 0, maxlife = 1;
            if (statusType == StatusType.Health) {
                life = MemoryReader.Health;
                maxlife = MemoryReader.MaxHealth;
            } else if (statusType == StatusType.Mana) {
                life = MemoryReader.Mana;
                maxlife = MemoryReader.MaxMana;
            } else if (statusType == StatusType.Experience) {
                int level = MemoryReader.Level;
                long baseExperience = GetExperience(level - 1);
                life = MemoryReader.Experience - baseExperience;
                maxlife = GetExperience(level) - baseExperience;
            }
            if (maxlife == 0) {
                life = 1;
                maxlife = 1;
            }

            try {
                bool visible = ProcessManager.IsTibiaActive();
                this.Invoke((MethodInvoker)delegate {
                    RefreshHUD(life, maxlife);
                    this.Visible = alwaysShow ? true : visible;
                });
            } catch {
            }
        }

        public override string GetHUD() {
            switch (statusType) {
                case StatusType.Health:
                    return "HealthBar";
                case StatusType.Mana:
                    return "ManaBar";
                case StatusType.Experience:
                    return "ExperienceBar";
            }
            return "StatusBar";
        }
    }
}
