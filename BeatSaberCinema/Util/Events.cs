﻿using System;

namespace BeatSaberCinema
{
	public static class Events
	{
		/// <summary>
		/// Indicates if Cinema will be doing something on the upcoming song (either play a video or modify the scene).
		/// Will be invoked as soon as the scene transition to the gameplay scene is initiated.
		/// </summary>
		public static Action<bool>? CinemaActivated;

		/// <summary>
		/// Used by CustomPlatforms to detect whether or not a custom platform should be loaded.
		/// Will be invoked as soon as the scene transition to the gameplay scene is initiated.
		/// </summary>
		public static Action<bool>? AllowCustomPlatform;

		internal static void InvokeSceneTransitionEvents(VideoConfig? videoConfig)
		{
			if (!SettingsStore.Instance.PluginEnabled || videoConfig == null)
			{
				CinemaActivated?.Invoke(false);
				AllowCustomPlatform?.Invoke(true);
				return;
			}

			var cinemaActivated = (videoConfig.IsPlayable || videoConfig.forceEnvironmentModifications == true);
			CinemaActivated?.Invoke(cinemaActivated);

			bool allowCustomPlatform;
			if (videoConfig.allowCustomPlatform == null)
			{
				//If the mapper didn't explicitly allow or disallow custom platforms, use global setting
				allowCustomPlatform = (!cinemaActivated || !SettingsStore.Instance.DisableCustomPlatforms);
			}
			else
			{
				//Otherwise use that setting instead of the global one
				allowCustomPlatform = (!cinemaActivated || videoConfig.allowCustomPlatform == true);
			}

			AllowCustomPlatform?.Invoke(allowCustomPlatform);
		}
	}
}