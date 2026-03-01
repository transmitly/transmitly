// ﻿﻿Copyright (c) Code Impressions, LLC. All Rights Reserved.
//  
//  Licensed under the Apache License, Version 2.0 (the "License")
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using AutoFixture;
using Moq;
using Transmitly.Channel.Configuration.Push;
using Transmitly.Tests;

namespace Transmitly.Channel.Push.Tests;

[TestClass()]
public class PushNotificationChannelTests : BaseUnitTest
{
	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldGuardAgainstNullContext()
	{
		var channel = new PushNotificationChannel(new PushNotificationChannelConfiguration());
		await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => channel.GenerateCommunicationAsync(null!));
	}

	[TestMethod()]
	public void SupportsPlatformIdentityType()
	{
		var tests = new List<(IPlatformIdentityAddress, bool)> {
			(new PlatformIdentityAddress("test", type: PlatformIdentityAddress.Types.DeviceToken()), true),
			(new PlatformIdentityAddress("test", type: PlatformIdentityAddress.Types.Topic()), true),
			(new PlatformIdentityAddress("fe595523a0c2965f9eabff921555df48-80df133c-5aab-4db4-bd03-b04331181664", type:PlatformIdentityAddress.Types.DeviceToken()), true),
			(new PlatformIdentityAddress("test", type: "other"), false),
			(new PlatformIdentityAddress("test"), false),
			(new PlatformIdentityAddress("fe595523a0c2965f9eabff921555df48-80df133c-5aab-4db4-bd03-b04331181664"), true)
		};

		foreach (var test in tests)
		{
			var channel = new PushNotificationChannel(new PushNotificationChannelConfiguration());
			var result = channel.SupportsIdentityAddress(test.Item1);
			Assert.AreEqual(test.Item2, result, test.Item1.Value + ":" + test.Item1.Type);
		}
	}

	[TestMethod]
	public void ShouldSetProvidedChannelProviderIds()
	{
		var list = fixture.Freeze<string[]>();
		var config = new PushNotificationChannelConfiguration();
		config.AddChannelProviderFilter(list);
		var channel = new PushNotificationChannel(config);
		CollectionAssert.AreEquivalent(list, channel.AllowedChannelProviderIds.ToArray());
	}

	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldRenderConfiguredDefaultAndPlatformContent()
	{
		var mockContext = fixture.Create<Mock<IDispatchCommunicationContext>>();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		mockContext.SetupGet(x => x.TemplateEngine).Returns(new UnitTestTemplateEngine());
		var context = mockContext.Object;

		var config = new PushNotificationChannelConfiguration();
		config.Title.AddStringTemplate("default-title");
		config.Body.AddStringTemplate("default-body");
		config.ImageUrl.AddStringTemplate("default-image");
		config
			.AddData("k", c => c.AddStringTemplate("v"))
			.AddData("k2", _ => Task.FromResult<string?>(null))
			.AddHeader("h", c => c.AddStringTemplate("hv"))
			.AddHeader("h2", _ => Task.FromResult<string?>(null));

		config.AddAndroid(android =>
		{
			android.Title.AddStringTemplate("android-title");
			android.Body.AddStringTemplate("android-body");
			android.ImageUrl.AddStringTemplate("android-image");
			android
				.AddData("ak", c => c.AddStringTemplate("av"))
				.AddData("ak2", _ => Task.FromResult<string?>(null))
				.AddHeader("ah", c => c.AddStringTemplate("ahv"))
				.AddHeader("ah2", _ => Task.FromResult<string?>(null))
				.AddCollapseId("collapse")
				.AddPriority(AndroidNotificationPriority.High)
				.AddTimeToLive(TimeSpan.FromMinutes(5))
				.AddTargetApplicationId("test-app")
				.AddAllowDeliveryBeforeFirstUnlock(true);
		});

		config.AddApple(apple =>
		{
			apple.Title.AddStringTemplate("apple-title");
			apple.Body.AddStringTemplate("apple-body");
			apple.ImageUrl.AddStringTemplate("apple-image");
			apple
				.AddData("apk", c => c.AddStringTemplate("apv"))
				.AddHeader("aph", c => c.AddStringTemplate("aphv"))
				.AddSubtitle("subtitle")
				.AddSubtitleLocalizationKey("sub-loc-key")
				.AddSubtitleLocalizationArguments("sa")
				.AddActionLocalizationKey("action-key")
				.AddBodyLocalizationKey("loc-key")
				.AddBodyLocalizationArguments("la")
				.AddTitleLocalizationKey("title-loc-key")
				.AddTitleLocalizationArguments("ta")
				.AddBadge(7)
				.AddSound("ding")
				.AddBackgroundUpdate(true)
				.AddContentMutable(true)
				.AddCategory("category")
				.AddThreadId("thread");
		});

		config.AddWeb(web =>
		{
			web.Title.AddStringTemplate("web-title");
			web.Body.AddStringTemplate("web-body");
			web.ImageUrl.AddStringTemplate("web-image");
			web.Icon.AddStringTemplate("web-icon");
			web.Badge.AddStringTemplate("web-badge");
			web.Language.AddStringTemplate("en-US");
			web
				.AddData("wk", c => c.AddStringTemplate("wv"))
				.AddHeader("wh", c => c.AddStringTemplate("whv"))
				.AddRenotify(true)
				.AddRequireInteraction(true)
				.AddSilent(false)
				.AddTag("web-tag")
				.AddTimestamp(new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero))
				.AddVibratePattern(100, 200)
				.AddDirection(WebPushDisplayDirection.Auto);
		});

		var channel = new PushNotificationChannel(config);
		var result = await channel.GenerateCommunicationAsync(context);

		Assert.AreEqual("default-title", result.Title);
		Assert.AreEqual("default-body", result.Body);
		Assert.AreEqual("default-image", result.ImageUrl);
		Assert.IsNotNull(result.Data);
		Assert.AreEqual("v", result.Data["k"]);
		Assert.IsFalse(result.Data.ContainsKey("k2"));
		Assert.IsNotNull(result.Headers);
		Assert.AreEqual("hv", result.Headers["h"]);
		Assert.IsFalse(result.Headers.ContainsKey("h2"));
		CollectionAssert.AreEquivalent(context.PlatformIdentities.SelectMany(m => m.Addresses).ToArray(), result.Recipient.ToArray());

		Assert.IsNotNull(result.Android);
		Assert.AreEqual("android-title", result.Android.Title);
		Assert.AreEqual("android-body", result.Android.Body);
		Assert.AreEqual("android-image", result.Android.ImageUrl);
		Assert.IsNotNull(result.Android.Data);
		Assert.AreEqual("av", result.Android.Data["ak"]);
		Assert.IsFalse(result.Android.Data.ContainsKey("ak2"));
		Assert.IsNotNull(result.Android.Headers);
		Assert.AreEqual("ahv", result.Android.Headers["ah"]);
		Assert.IsFalse(result.Android.Headers.ContainsKey("ah2"));
		Assert.AreEqual("collapse", result.Android.CollapseId);
		Assert.AreEqual(AndroidNotificationPriority.High, result.Android.Priority);
		Assert.AreEqual(TimeSpan.FromMinutes(5), result.Android.TimeToLive);
		Assert.AreEqual("test-app", result.Android.TargetApplicationId);
		Assert.IsTrue(result.Android.AllowDeliveryBeforeFirstUnlock);

		Assert.IsNotNull(result.Apple);
		Assert.AreEqual("apple-title", result.Apple.Title);
		Assert.AreEqual("apple-body", result.Apple.Body);
		Assert.AreEqual("apple-image", result.Apple.ImageUrl);
		Assert.IsNotNull(result.Apple.Data);
		Assert.AreEqual("apv", result.Apple.Data["apk"]);
		Assert.IsNotNull(result.Apple.Headers);
		Assert.AreEqual("aphv", result.Apple.Headers["aph"]);
		Assert.AreEqual("subtitle", result.Apple.Subtitle);
		Assert.AreEqual("sub-loc-key", result.Apple.SubtitleLocalizationKey);
		CollectionAssert.AreEquivalent(new[] { "sa" }, result.Apple.SubtitleLocalizationArguments.ToArray());
		Assert.AreEqual("action-key", result.Apple.ActionLocalizationKey);
		Assert.AreEqual("loc-key", result.Apple.BodyLocalizationKey);
		CollectionAssert.AreEquivalent(new[] { "la" }, result.Apple.BodyLocalizationArguments!.ToArray());
		Assert.AreEqual("title-loc-key", result.Apple.TitleLocalizationKey);
		CollectionAssert.AreEquivalent(new[] { "ta" }, result.Apple.TitleLocalizationArguments!.ToArray());
		Assert.AreEqual(7, result.Apple.Badge);
		Assert.AreEqual("ding", result.Apple.Sound);
		Assert.IsTrue(result.Apple.IsBackgroundUpdate);
		Assert.IsTrue(result.Apple.IsContentMutable);
		Assert.AreEqual("category", result.Apple.Category);
		Assert.AreEqual("thread", result.Apple.ThreadId);

		Assert.IsNotNull(result.Web);
		Assert.AreEqual("web-title", result.Web.Title);
		Assert.AreEqual("web-body", result.Web.Body);
		Assert.AreEqual("web-image", result.Web.ImageUrl);
		Assert.AreEqual("web-icon", result.Web.Icon);
		Assert.AreEqual("web-badge", result.Web.Badge);
		Assert.AreEqual("en-US", result.Web.Language);
		Assert.IsNotNull(result.Web.Data);
		Assert.AreEqual("wv", result.Web.Data["wk"]);
		Assert.IsNotNull(result.Web.Headers);
		Assert.AreEqual("whv", result.Web.Headers["wh"]);
		Assert.IsTrue(result.Web.Renotify);
		Assert.IsTrue(result.Web.RequireInteraction);
		Assert.IsFalse(result.Web.IsSilent);
		Assert.AreEqual("web-tag", result.Web.Tag);
		Assert.AreEqual(new DateTimeOffset(2025, 1, 1, 1, 2, 3, TimeSpan.Zero), result.Web.Timestamp);
		CollectionAssert.AreEqual(new[] { 100, 200 }, result.Web.VibratePattern!.ToArray());
		Assert.AreEqual(WebPushDisplayDirection.Auto, result.Web.Direction);
	}

	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldCreatePlatformContentWhenOnlyPlatformOptionsAreConfigured()
	{
		var mockContext = fixture.Create<Mock<IDispatchCommunicationContext>>();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		mockContext.SetupGet(x => x.TemplateEngine).Returns(new UnitTestTemplateEngine());
		var context = mockContext.Object;

		var config = new PushNotificationChannelConfiguration();
		config.AddAndroid(android =>
		{
			android
				.AddPriority(AndroidNotificationPriority.Normal)
				.AddAllowDeliveryBeforeFirstUnlock(true);
		});

		config.AddApple(apple =>
		{
			apple
				.AddBadge(1)
				.AddThreadId("thread-id");
		});

		config.AddWeb(web =>
		{
			web
				.AddRenotify(true)
				.AddTag("web-tag");
		});

		var channel = new PushNotificationChannel(config);
		var result = await channel.GenerateCommunicationAsync(context);

		Assert.IsNotNull(result.Android);
		Assert.AreEqual(AndroidNotificationPriority.Normal, result.Android.Priority);
		Assert.IsTrue(result.Android.AllowDeliveryBeforeFirstUnlock);

		Assert.IsNotNull(result.Apple);
		Assert.AreEqual(1, result.Apple.Badge);
		Assert.AreEqual("thread-id", result.Apple.ThreadId);

		Assert.IsNotNull(result.Web);
		Assert.IsTrue(result.Web.Renotify);
		Assert.AreEqual("web-tag", result.Web.Tag);
	}

	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldRenderWebStringHelpersAndIgnoreWhitespace()
	{
		var mockContext = fixture.Create<Mock<IDispatchCommunicationContext>>();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		mockContext.SetupGet(x => x.TemplateEngine).Returns(new UnitTestTemplateEngine());
		var context = mockContext.Object;

		var config = new PushNotificationChannelConfiguration();
		config.AddWeb(web =>
		{
			web
				.AddIcon("web-icon")
				.AddBadge("web-badge")
				.AddLanguage("en-US");
		});

		var channel = new PushNotificationChannel(config);
		var result = await channel.GenerateCommunicationAsync(context);

		Assert.IsNotNull(result.Web);
		Assert.AreEqual("web-icon", result.Web.Icon);
		Assert.AreEqual("web-badge", result.Web.Badge);
		Assert.AreEqual("en-US", result.Web.Language);

		var configWithWhitespace = new PushNotificationChannelConfiguration();
		configWithWhitespace.AddWeb(web =>
		{
			web
				.AddIcon("  ")
				.AddBadge(null)
				.AddLanguage(string.Empty);
		});

		var resultWithWhitespace = await new PushNotificationChannel(configWithWhitespace).GenerateCommunicationAsync(context);
		Assert.IsNull(resultWithWhitespace.Web);
	}

	[TestMethod]
	public async Task GenerateCommunicationAsyncShouldLeavePlatformContentNullWhenNotConfigured()
	{
		var mockContext = fixture.Create<Mock<IDispatchCommunicationContext>>();
		mockContext.Setup(x => x.ContentModel!.Resources).Returns([]);
		mockContext.SetupGet(x => x.TemplateEngine).Returns(new UnitTestTemplateEngine());
		var context = mockContext.Object;

		var config = new PushNotificationChannelConfiguration();
		config.Title.AddStringTemplate("title");
		var channel = new PushNotificationChannel(config);

		var result = await channel.GenerateCommunicationAsync(context);

		Assert.IsNull(result.Android);
		Assert.IsNull(result.Apple);
		Assert.IsNull(result.Web);
	}
}
