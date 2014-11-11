﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ZodiacGlass.FFXIV;

namespace ZodiacGlass
{
    internal class OverlayViewModel : ViewModelBase
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private const int AdditionLifeTime = 20000;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FFXIVMemoryReader glass;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private OverlayDisplayMode mode;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FFXIVMemoryObserver observer;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool ignoreNextMainHandAddition;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool ignoreNextOffHandAddition;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int mainHandAddition;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int offHandAddition;

        public OverlayViewModel()
        {


        }

        public FFXIVMemoryReader MemoryReader
        {
            get
            {
                return this.glass;
            }
            set
            {
                if (this.observer != null)
                {
                    this.observer.EquippedMainHandLightAmountChanged -= this.OnEquippedMainHandLightAmountChanged;
                    this.observer.EquippedOffHandLightAmountChanged -= this.OnEquippedOffHandLightAmountChanged;
                    this.observer.EquippedMainHandIDChanged -= this.OnEquippedMainHandIDChanged;
                    this.observer.EquippedOffHandIDChanged -= this.OnEquippedOffHandIDChanged;
                    this.observer.Dispose();
                    this.observer = null;
                }


                this.glass = value;

                if (this.glass != null)
                {
                    this.observer = new FFXIVMemoryObserver(this.glass);

                    this.observer.EquippedMainHandLightAmountChanged += this.OnEquippedMainHandLightAmountChanged;
                    this.observer.EquippedOffHandLightAmountChanged += this.OnEquippedOffHandLightAmountChanged;
                    this.observer.EquippedMainHandIDChanged += this.OnEquippedMainHandIDChanged;
                    this.observer.EquippedOffHandIDChanged += this.OnEquippedOffHandIDChanged;
                }


                this.NotifyPropertyChanged(() => this.OverlayVisibility);
                this.NotifyPropertyChanged(() => this.ClassSymbolUri);
                this.NotifyPropertyChanged(() => this.EquippedMainHandLightAmount);
                this.NotifyPropertyChanged(() => this.EquippedOffHandLightAmount);
                this.NotifyPropertyChanged(() => this.OffHandVisibility);
            }
        }

        private void OnEquippedMainHandLightAmountChanged(object sender, ValueChangedEventArgs<int> e)
        {
            this.NotifyPropertyChanged(() => this.EquippedMainHandLightAmount);

            if (!this.ignoreNextMainHandAddition)
            {
                this.MainHandAddition = e.NewValue - e.OldValue;

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(AdditionLifeTime);
                    this.MainHandAddition = 0;
                });
            }
            else
            {
                this.ignoreNextMainHandAddition = false;
            }
        }

        private void OnEquippedOffHandLightAmountChanged(object sender, ValueChangedEventArgs<int> e)
        {
            this.NotifyPropertyChanged(() => this.EquippedOffHandLightAmount);

            if (!this.ignoreNextOffHandAddition)
            {
                this.OffHandAddition = e.NewValue - e.OldValue;

                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(AdditionLifeTime);
                    this.OffHandAddition = 0;
                });
            }
            else
            {
                this.ignoreNextOffHandAddition = false;
            }
        }

        private void OnEquippedMainHandIDChanged(object sender, ValueChangedEventArgs<int> e)
        {
            this.NotifyPropertyChanged(() => this.OverlayVisibility);
            this.NotifyPropertyChanged(() => this.MainHandVisibility);
            this.NotifyPropertyChanged(() => this.SeparatorVisibility);
            this.NotifyPropertyChanged(() => this.ClassSymbolUri);
            this.ignoreNextMainHandAddition = true;
        }

        private void OnEquippedOffHandIDChanged(object sender, ValueChangedEventArgs<int> e)
        {
            this.NotifyPropertyChanged(() => this.OverlayVisibility);
            this.NotifyPropertyChanged(() => this.OffHandVisibility);
            this.NotifyPropertyChanged(() => this.SeparatorVisibility);
            this.NotifyPropertyChanged(() => this.ClassSymbolUri);
            this.ignoreNextOffHandAddition = true;
        }

        public Uri ClassSymbolUri
        {

            get
            {

                if (this.glass != null)
                {
                    string className = null;

                    if (this.glass.GetEquippedOffHandID() == (int)FFXIVNovusWeapon.HolyShieldNovus)
                    {
                        className = "paladin";
                    }
                    else
                    {
                        switch ((FFXIVNovusWeapon)this.glass.GetEquippedMainHandID())
                        {
                            case FFXIVNovusWeapon.CurtanaNovus:
                                className = "paladin";
                                break;
                            case FFXIVNovusWeapon.SphairaiNovus:
                                className = "monk";
                                break;
                            case FFXIVNovusWeapon.BravuraNovus:
                                className = "warrior";
                                break;
                            case FFXIVNovusWeapon.GaeBolgNovus:
                                className = "dragoon";
                                break;
                            case FFXIVNovusWeapon.ArtemisBowNovus:
                                className = "bard";
                                break;
                            case FFXIVNovusWeapon.ThyrusNovus:
                                className = "whitemage";
                                break;
                            case FFXIVNovusWeapon.StardustRodNovus:
                                className = "blackmage";
                                break;
                            case FFXIVNovusWeapon.TheVeilofWiyuNovus:
                                className = "summoner";
                                break;
                            case FFXIVNovusWeapon.OmnilexNovus:
                                className = "scholar";
                                break;
                            case FFXIVNovusWeapon.YoshimitsuNovus:
                                className = "ninja";
                                break;
                        }
                    }

                    if (className != null)
                        return new Uri(string.Format("pack://application:,,,/Zodiac Glass;component/Resources/classimages/{0}.png", className));
                }

                return null;
            }

        }

        public string EquippedMainHandLightAmount
        {
            get
            {
                int val = 0;

                if (this.glass != null)
                    val = this.glass.GetEquippedMainHandLightAmount();

                return this.Mode == OverlayDisplayMode.Normal ? val.ToString() : string.Format("{0} %", Math.Round(100 * (float)val / 2000, 2));
            }
        }

        public string EquippedOffHandLightAmount
        {
            get
            {
                int val = 0;

                if (this.glass != null)
                    val = this.glass.GetEquippedOffHandLightAmount();

                return this.Mode == OverlayDisplayMode.Normal ? val.ToString() : string.Format("{0} %", Math.Round(100 * (float)val / 2000, 2));
            }
        }

        public OverlayDisplayMode Mode
        {
            get
            {
                return mode;
            }
            set
            {

                if (mode != value)
                {
                    mode = value;
                    this.NotifyPropertyChanged(() => this.Mode);
                    this.NotifyPropertyChanged(() => this.EquippedMainHandLightAmount);
                    this.NotifyPropertyChanged(() => this.EquippedOffHandLightAmount);
                }
            }
        }



        public int MainHandAddition
        {
            get
            {
                return this.mainHandAddition;
            }
            set
            {
                this.mainHandAddition = value;
                this.NotifyPropertyChanged(() => this.MainHandAddition);
                this.NotifyPropertyChanged(() => this.MainHandAdditionVisibility);
            }
        }

        public int OffHandAddition
        {
            get
            {
                return this.offHandAddition;
            }
            set
            {
                this.offHandAddition = value;
                this.NotifyPropertyChanged(() => this.OffHandAddition);
                this.NotifyPropertyChanged(() => this.OffHandAdditionVisibility);
            }
        }
        public Visibility MainHandAdditionVisibility
        {
            get
            {
                return this.MainHandVisibility == Visibility.Visible && this.mainHandAddition > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility OffHandAdditionVisibility
        {
            get
            {
                return this.OffHandVisibility == Visibility.Visible && this.offHandAddition > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility SeparatorVisibility
        {
            get
            {
                return this.MainHandVisibility == Visibility.Visible && this.OffHandVisibility == Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility MainHandVisibility
        {
            get
            {
                return this.glass != null && Enum.IsDefined(typeof(FFXIVNovusWeapon), this.glass.GetEquippedMainHandID()) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility OffHandVisibility
        {
            get
            {
                return this.glass != null && (FFXIVNovusWeapon)this.glass.GetEquippedOffHandID() == FFXIVNovusWeapon.HolyShieldNovus ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility OverlayVisibility
        {
            get
            {
                return this.ClassSymbolUri != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}