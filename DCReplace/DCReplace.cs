using System;
using Exiled.API.Enums;
using Exiled.API.Features;

namespace DCReplace
{
	public class DCReplace : Plugin<Config>
	{
		private EventHandlers ev;
		public override string Author => "DGvagabond";
		public override string Name => "DcReplace";
		public override PluginPriority Priority => PluginPriority.Medium;
		public override Version Version { get; } = new Version(1, 0, 0, 0);
		public override Version RequiredExiledVersion { get; } = new Version(2, 11, 1);
		
		public override void OnEnabled()
		{
			base.OnEnabled();

			if (!Config.IsEnabled) return;

			ev = new EventHandlers();

			Exiled.Events.Handlers.Player.Destroying += ev.OnPlayerDestroying;
			Exiled.Events.Handlers.Scp106.Containing += ev.OnContain106;
			Exiled.Events.Handlers.Server.RoundStarted += ev.OnRoundStart;
		}

		public override void OnDisabled()
		{
			base.OnDisabled();

			Exiled.Events.Handlers.Player.Destroying -= ev.OnPlayerDestroying;
			Exiled.Events.Handlers.Scp106.Containing -= ev.OnContain106;
			Exiled.Events.Handlers.Server.RoundStarted -= ev.OnRoundStart;

			ev = null;
		}
	}
}
