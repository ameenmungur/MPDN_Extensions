// This file is a part of MPDN Extensions.
// https://github.com/zachsaw/MPDN_Extensions
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.
// 

using System;
using System.Linq;
using System.Windows.Forms;
using Mpdn.Extensions.Framework;
using Mpdn.Extensions.PlayerExtensions.GitHub;
using Mpdn.Extensions.PlayerExtensions.Playlist;

namespace Mpdn.Extensions.PlayerExtensions
{
    public class MouseControl : PlayerExtension<MouseControlSettings, MouseControlConfigDialog>
    {
        public override ExtensionUiDescriptor Descriptor
        {
            get
            {
                return new ExtensionUiDescriptor
                {
                    Guid = new Guid("DCF7797B-9D36-41F3-B28C-0A92793B94F5"),
                    Name = "Mouse Control",
                    Description =
                        string.Format("Use mouse {0}forward/back buttons to navigate chapters/playlist/folder",
                            Settings.EnableMouseWheelSeek ? "wheel to seek and " : string.Empty)
                };
            }
        }

        protected override string ConfigFileName
        {
            get { return "Example.MouseControl"; }
        }

        public override void Initialize()
        {
            base.Initialize();

            Player.MouseClick += PlayerMouseClick;
            Player.MouseWheel += PlayerMouseWheel;
        }

        public override void Destroy()
        {
            base.Destroy();

            Player.MouseClick -= PlayerMouseClick;
            Player.MouseWheel -= PlayerMouseWheel;
        }

        private void PlayerMouseWheel(object sender, PlayerControlEventArgs<MouseEventArgs> e)
        {
            if (!Settings.EnableMouseWheelSeek)
                return;

            var pos = Media.Position;
            pos += e.InputArgs.Delta*1000000/40;
            pos = Math.Max(pos, 0);
            Media.Seek(pos);
            e.Handled = true;
        }

        private void PlayerMouseClick(object sender, PlayerControlEventArgs<MouseEventArgs> e)
        {
            if (e.InputArgs.Button == MouseButtons.Middle)
            {
                ToggleMode();
                return;
            }

            if (Player.State == PlayerState.Closed)
                return;

            var chapters = Media.Chapters.OrderBy(chapter => chapter.Position);

            var pos = Media.Position;

            bool next;
            switch (e.InputArgs.Button)
            {
                case MouseButtons.XButton2:
                    next = true;
                    break;
                case MouseButtons.XButton1:
                    next = false;
                    break;
                default:
                    return;
            }

            var nextChapter = next
                ? chapters.SkipWhile(chapter => chapter.Position < pos).FirstOrDefault()
                : chapters.TakeWhile(chapter => chapter.Position < Math.Max(pos - 1000000, 0)).LastOrDefault();
            if (nextChapter != null)
            {
                Media.Seek(nextChapter.Position);
                return;
            }

            if (PlaylistForm.PlaylistCount <= 1)
            {
                switch (e.InputArgs.Button)
                {
                    case MouseButtons.XButton2:
                        SendKeys.Send("^{PGDN}");
                        break;
                    case MouseButtons.XButton1:
                        SendKeys.Send("^{PGUP}");
                        break;
                }
            }
            else
            {
                switch (e.InputArgs.Button)
                {
                    case MouseButtons.XButton2:
                        SendKeys.Send("^%n");
                        break;
                    case MouseButtons.XButton1:
                        SendKeys.Send("^%b");
                        break;
                }
            }
        }

        private void ToggleMode()
        {
            if (!Settings.EnableMiddleClickFsToggle)
                return;

            Player.FullScreenMode.Active = !Player.FullScreenMode.Active;
        }
    }

    public class MouseControlSettings
    {
        public MouseControlSettings()
        {
            EnableMouseWheelSeek = false;
            EnableMiddleClickFsToggle = false;
        }

        public bool EnableMouseWheelSeek { get; set; }
        public bool EnableMiddleClickFsToggle { get; set; }
    }
}
