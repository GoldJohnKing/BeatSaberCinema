﻿using System;
using System.Linq;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatSaberCinema
{
	[HarmonyBefore("com.kyle1413.BeatSaber.BS-Utils")]
	[HarmonyPatch(typeof(StandardLevelScenesTransitionSetupDataSO), nameof(StandardLevelScenesTransitionSetupDataSO.Init))]
	[UsedImplicitly]
	// ReSharper disable once InconsistentNaming
	internal static class StandardLevelScenesTransitionSetupDataSOInit
	{
		[UsedImplicitly]
		public static void Prefix(IDifficultyBeatmap difficultyBeatmap, ref OverrideEnvironmentSettings overrideEnvironmentSettings)
		{
			//Wrap all of it in try/catch so an exception would not prevent the player from playing songs
			try
			{
				PlaybackController.Instance.SceneTransitionInitCalled();
				VideoMenu.instance.SetSelectedLevel(difficultyBeatmap.level);

				var overrideEnvironmentEnabled = SettingsStore.Instance.OverrideEnvironment;
				var environmentInfoSo = difficultyBeatmap.GetEnvironmentInfo();

				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				if (overrideEnvironmentSettings != null && overrideEnvironmentSettings.overrideEnvironments)
				{
					environmentInfoSo = overrideEnvironmentSettings.GetOverrideEnvironmentInfoForType(environmentInfoSo.environmentType);
				}

				var environmentWhitelist = new[] {"BigMirrorEnvironment", "OriginsEnvironment", "BTSEnvironment", "KDAEnvironment", "RocketEnvironment", "DragonsEnvironment", "LinkinParkEnvironment", "KaleidoscopeEnvironment", "GlassDesertEnvironment"};
				if (environmentWhitelist.Contains(environmentInfoSo.serializedName))
				{
					Log.Debug("Environment in whitelist");
					overrideEnvironmentEnabled = false;
				}

				var video = PlaybackController.Instance.VideoConfig;
				if (video == null || !video.IsPlayable)
				{
					Log.Debug($"No video or not playable, DownloadState: {video?.DownloadState}");
					overrideEnvironmentEnabled = false;
				} else if (video.disableBigMirrorOverride != null && video.disableBigMirrorOverride == true)
				{
					Log.Debug("Override disabled via config");
					overrideEnvironmentEnabled = false;
				}

				if (!overrideEnvironmentEnabled)
				{
					Log.Debug("Skipping environment override");
					return;
				}

				var bigMirrorEnvInfo = Resources.FindObjectsOfTypeAll<EnvironmentInfoSO>().First(x => x.serializedName == "BigMirrorEnvironment");
				if (bigMirrorEnvInfo == null)
				{
					Log.Warn("Did not find big mirror env");
					return;
				}

				var bigMirrorOverrideSettings = new OverrideEnvironmentSettings {overrideEnvironments = true};
				bigMirrorOverrideSettings.SetEnvironmentInfoForType(bigMirrorEnvInfo.environmentType, bigMirrorEnvInfo);
				overrideEnvironmentSettings = bigMirrorOverrideSettings;
				Log.Info("Overwriting environment to Big Mirror");
			}
			catch (Exception e)
			{
				Log.Warn(e);
			}
		}
	}



	[HarmonyBefore("com.kyle1413.BeatSaber.BS-Utils")]
	[HarmonyPatch(typeof(MissionLevelScenesTransitionSetupDataSO), "Init")]
	[UsedImplicitly]
	// ReSharper disable once InconsistentNaming
	internal static class MissionLevelScenesTransitionSetupDataSOInit
	{
		[UsedImplicitly]
		private static void Prefix(IDifficultyBeatmap difficultyBeatmap)
		{
			try
			{
				var overrideSettings = new OverrideEnvironmentSettings();
				StandardLevelScenesTransitionSetupDataSOInit.Prefix(difficultyBeatmap, ref overrideSettings);
			}
			catch (Exception e)
			{
				Log.Warn(e);
			}
		}
	}
}