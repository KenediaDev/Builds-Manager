using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Blish_HUD;
using Blish_HUD.Controls;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using Blish_HUD.Modules.Managers;
using Blish_HUD.Content;

namespace Kenedia.Modules.BuildsManager.Classes
{
    class AsyncImage
    {
		public class Icon : IDisposable
		{
			void IDisposable.Dispose() { }
			private bool disposed = false;
			public void Dispose()
			{
				if (!disposed)
				{
					disposed = true;
					Texture?.Dispose();
					Texture = null;
				}
			}

			public string Path;
			public string Url;
			private bool _LoadQueued;
			private AsyncTexture2D _Texture;
			public AsyncTexture2D Texture
			{
				set 
				{
					_Texture = value;
				}
				get
				{
					if (!_LoadQueued)
					{
						_LoadQueued = true;

						GameService.Graphics.QueueMainThreadRender((graphicsDevice) =>
						{
							_Texture.SwapTexture(TextureUtil.FromStreamPremultiplied(graphicsDevice, new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)));
						});
					}

					return _Texture;
				}
			}
		}
	}
}
