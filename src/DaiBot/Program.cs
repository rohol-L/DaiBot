using DaiBot.Core.Application;
using DaiBot.Core.Interface;
using DaiBot.Core.Utils;
using DaiBot.Manager;
using DaiBot.Service;
using DaiBot.Services;

var builder = DaiBotApplication.CreateBuilder(args);
builder.Services.AddSingleton<BotConfigService, IBotConfig>();
builder.Services.AddSingleton<StorageService, IStorage>();
builder.Services.AddSingleton<DbCollectionService, IDbCollection>();

var app = builder.Build();

PluginManager.Start(app.Plugins);
PluginManager.ReloadPlugin();

app.Start();

MyConsole.StartLoop();