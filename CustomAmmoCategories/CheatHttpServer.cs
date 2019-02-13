using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;

namespace CustAmmoCategories
{
    public class ThreadWork
    {
        public class CDefItem
        {
            public string name { get; set; }
            public int price { get; set; }
            public BattleTech.ShopItemType type { get; set; }
            public int count { get; set; }
        }
        public class CSetReputation
        {
            public string faction { get; set; }
            public int reputation { get; set; }
        }
        private static string GetMimeType(string ext)
        {
            switch (ext)
            {
                case ".html": return "text/html";
                case ".htm": return "text/html";
                case ".txt": return "text/plain";
                case ".jpe": return "image/jpeg";
                case ".jpeg": return "image/jpeg";
                case ".jpg": return "image/jpeg";
                case ".js": return "application/javascript";
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".bmp": return "image/bmp";
                case ".ico": return "image/x-icon";
            }
            return "application/octed-stream";
        }
        private static void SendError(ref HttpListenerResponse response, int Code, string text)
        {
            // Получаем строку вида "200 OK"
            // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
            response.StatusCode = Code;
            response.ContentType = "text/html";
            string Html = "<html><body><h1>" + text + "</h1></body></html>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(Html);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            // Закроем соединение
            output.Write(buffer, 0, buffer.Length);
            output.Close();
            response.Close();
        }
        private static void SendResponce(ref HttpListenerResponse response, object jresp)
        {
            // Получаем строку вида "200 OK"
            // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
            response.StatusCode = 200;
            response.ContentType = "application/json";
            string Html = JsonConvert.SerializeObject(jresp);
            // Приведем строку к виду массива байт
            byte[] Buffer = Encoding.UTF8.GetBytes(Html);
            // Отправим его клиенту
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(Html);
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            // Закроем соединение
            output.Write(buffer, 0, buffer.Length);
            output.Close();
            response.Close();
        }

        public static void DoWork()
        {
            CustomAmmoCategoriesLog.Log.LogWrite("Initing http server " + CustomAmmoCategories.Settings.modHTTPServer + "...\n");
            if (CustomAmmoCategories.Settings.modHTTPServer == false) { return; }
            HttpListener listener = new HttpListener();
            CustomAmmoCategoriesLog.Log.LogWrite("Prefix " + CustomAmmoCategories.Settings.modHTTPListen + "...\n");
            listener.Prefixes.Add(CustomAmmoCategories.Settings.modHTTPListen);
            listener.Start();
            string assemblyFile = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                string filename = request.Url.AbsolutePath;
                if (filename == "/") { filename = "index.html"; };
                filename = Path.Combine(CustomAmmoCategoriesLog.Log.BaseDirectory, Path.GetFileName(filename));
                CustomAmmoCategoriesLog.Log.LogWrite("Base directory " + CustomAmmoCategoriesLog.Log.BaseDirectory + " Access '" + filename + "'\n");
                if (File.Exists(filename))
                {
                    CustomAmmoCategoriesLog.Log.LogWrite("File exists\n");
                    response.ContentType = GetMimeType(Path.GetExtension(filename));
                    FileStream FS;
                    try
                    {
                        FS = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                    catch (Exception)
                    {
                        // Если случилась ошибка, посылаем клиенту ошибку 500
                        SendError(ref response, 500, "Не могу открыть " + filename);
                        return;
                    }
                    Stream output = response.OutputStream;
                    byte[] Buffer = new byte[1024];
                    // Переменная для хранения количества байт, принятых от клиента
                    int Count = 0;
                    // Пока не достигнут конец файла
                    while (FS.Position < FS.Length)
                    {
                        // Читаем данные из файла
                        Count = FS.Read(Buffer, 0, Buffer.Length);
                        // И передаем их клиенту
                        output.Write(Buffer, 0, Count);
                    }
                    // Закроем файл и соединение
                    FS.Close();
                    output.Close();
                    response.Close();
                    CustomAmmoCategoriesLog.Log.LogWrite("File out\n");
                    continue;
                }
                CustomAmmoCategoriesLog.Log.LogWrite("Get data:'" + Path.GetFileName(filename) + "'\n");
                if (Path.GetFileName(filename) == "getreputation")
                {
                    CustomAmmoCategoriesLog.Log.LogWrite("Запрос на получение репутации\n");
                    System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
                    BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
                    if (gameInstance == null)
                    {
                        jresp["error"] = "Не могу получить инстанс игры";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    BattleTech.SimGameState gameState = gameInstance.Simulation;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
                    if (gameState == null)
                    {
                        jresp["error"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN))
                    {
                        jresp["error"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
                    }
                    var factions = gameState.FactionsDict;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен список фракций\n");
                    foreach (var pFaction in factions)
                    {
                        jresp[pFaction.Key.ToString()] = gameState.GetRawReputation(pFaction.Key).ToString();
                    }
                    SendResponce(ref response, jresp);
                    continue;
                }
                if (Path.GetFileName(filename) == "setreputation")
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        string data = reader.ReadToEnd();
                        CSetReputation setrep = JsonConvert.DeserializeObject<CSetReputation>(data);
                        CustomAmmoCategoriesLog.Log.LogWrite("Запрос на установку репутации\n");
                        System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
                        BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
                        CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
                        if (gameInstance == null)
                        {
                            jresp["error"] = "Не могу получить инстанс игры";
                            SendResponce(ref response, jresp);
                            continue;
                        }
                        BattleTech.SimGameState gameState = gameInstance.Simulation;
                        CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
                        if (gameState == null)
                        {
                            jresp["error"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
                            SendResponce(ref response, jresp);
                            continue;
                        }
                        if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN))
                        {
                            jresp["error"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
                        }
                        var factions = gameState.FactionsDict;
                        CustomAmmoCategoriesLog.Log.LogWrite("Получен список фракций\n");
                        foreach (var pFaction in factions)
                        {
                            if (setrep.faction.Equals(pFaction.Key.ToString()))
                            {
                                gameState.SetReputation(pFaction.Key, setrep.reputation, BattleTech.StatCollection.StatOperation.Set);
                            }
                            jresp[pFaction.Key.ToString()] = gameState.GetRawReputation(pFaction.Key).ToString();
                        }
                        SendResponce(ref response, jresp);
                        continue;
                    }
                }
                if (Path.GetFileName(filename) == "listitems")
                {
                    CustomAmmoCategoriesLog.Log.LogWrite("Запрос на перечисление пилотов предмета\n");
                    System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
                    BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
                    if (gameInstance == null)
                    {
                        jresp["error"] = "Не могу получить инстанс игры";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    //gameInstance.DataManager
                    /*BattleTech.SimGameState gameState = gameInstance.Simulation;
                   CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
                    if (gameState == null)
                    {
                        jresp["error"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN))
                    {
                        jresp["error"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
                    }*/
                    BattleTech.Data.DataManager dataManager = gameInstance.DataManager;
                    if (dataManager == null)
                    {
                        jresp["error"] = "Не могу получить дата менеджер";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    List<CDefItem> items = new List<CDefItem>();
                    List<CDefItem> weapons = new List<CDefItem>();
                    foreach (var wDef in dataManager.WeaponDefs)
                    {
                        CDefItem itm = new CDefItem();
                        itm.name = wDef.Key;
                        itm.price = wDef.Value.Description.Cost;
                        itm.type = BattleTech.ShopItemType.Weapon;
                        itm.count = 1;
                        weapons.Add(itm);
                    }
                    weapons.Sort((x, y) => x.name.CompareTo(y.name));
                    List<CDefItem> amunitionBoxes = new List<CDefItem>();
                    foreach (var abDef in dataManager.AmmoBoxDefs)
                    {
                        CDefItem itm = new CDefItem();
                        itm.name = abDef.Key;
                        itm.price = abDef.Value.Description.Cost;
                        itm.type = BattleTech.ShopItemType.AmmunitionBox;
                        itm.count = 1;
                        amunitionBoxes.Add(itm);
                    }
                    amunitionBoxes.Sort((x, y) => x.name.CompareTo(y.name));
                    List<CDefItem> heatSinks = new List<CDefItem>();
                    foreach (var hsDef in dataManager.HeatSinkDefs)
                    {
                        CDefItem itm = new CDefItem();
                        itm.name = hsDef.Key;
                        itm.price = hsDef.Value.Description.Cost;
                        itm.type = BattleTech.ShopItemType.HeatSink;
                        itm.count = 1;
                        heatSinks.Add(itm);
                    }
                    heatSinks.Sort((x, y) => x.name.CompareTo(y.name));
                    List<CDefItem> jumpJets = new List<CDefItem>();
                    foreach (var jjDef in dataManager.JumpJetDefs)
                    {
                        CDefItem itm = new CDefItem();
                        itm.name = jjDef.Key;
                        itm.price = jjDef.Value.Description.Cost;
                        itm.type = BattleTech.ShopItemType.JumpJet;
                        itm.count = 1;
                        jumpJets.Add(itm);
                    }
                    jumpJets.Sort((x, y) => x.name.CompareTo(y.name));
                    List<CDefItem> upgrades = new List<CDefItem>();
                    foreach (var uDef in dataManager.UpgradeDefs)
                    {
                        CDefItem itm = new CDefItem();
                        itm.name = uDef.Key;
                        itm.price = uDef.Value.Description.Cost;
                        itm.type = BattleTech.ShopItemType.Upgrade;
                        itm.count = 1;
                        upgrades.Add(itm);
                    }
                    upgrades.Sort((x, y) => x.name.CompareTo(y.name));
                    List<CDefItem> mechs = new List<CDefItem>();
                    foreach (var mDef in dataManager.MechDefs)
                    {
                        CDefItem itm = new CDefItem();
                        itm.name = mDef.Key;
                        itm.price = mDef.Value.Description.Cost;
                        itm.type = BattleTech.ShopItemType.Mech;
                        itm.count = 1;
                        mechs.Add(itm);
                    }
                    mechs.Sort((x, y) => x.name.CompareTo(y.name));
                    items.AddRange(weapons);
                    items.AddRange(amunitionBoxes);
                    items.AddRange(heatSinks);
                    items.AddRange(upgrades);
                    items.AddRange(jumpJets);
                    items.AddRange(mechs);
                    SendResponce(ref response, items);
                    continue;
                }
                if (Path.GetFileName(filename) == "getchassisjson")
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        CustomAmmoCategoriesLog.Log.LogWrite("Запрос на получение файла шасси\n");
                        System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
                        BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
                        CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
                        if (gameInstance == null)
                        {
                            jresp["error"] = "Не могу получить инстанс игры";
                            SendResponce(ref response, jresp);
                            continue;
                        }
                        BattleTech.SimGameState gameState = gameInstance.Simulation;
                        CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
                        if (gameState == null)
                        {
                            jresp["error"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
                            SendResponce(ref response, jresp);
                            continue;
                        }
                        if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN))
                        {
                            jresp["error"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
                        }
                        BattleTech.Data.DataManager dataManager = gameInstance.DataManager;
                        if (dataManager == null)
                        {
                            jresp["error"] = "Не могу получить дата менеджер";
                            SendResponce(ref response, jresp);
                            continue;
                        }
                        string data = reader.ReadToEnd();
                        data = "mechdef_cyclops_CP-10-Z";
                        string jchassi = "{}";
                        bool chassi_found = false;
                        foreach (var mDef in dataManager.MechDefs)
                        {
                            if (mDef.Key == data)
                            {
                                jchassi = mDef.Value.Chassis.ToJSON();
                                chassi_found = true;
                            }
                        }
                        if (chassi_found == false)
                        {
                            jresp["error"] = "Не могу найти такое шасси";
                            SendResponce(ref response, jresp);
                            continue;
                        }
                        response.StatusCode = 200;
                        response.ContentType = "application/json";
                        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(jchassi);
                        response.ContentLength64 = buffer.Length;
                        Stream output = response.OutputStream;
                        // Закроем соединение
                        output.Write(buffer, 0, buffer.Length);
                        output.Close();
                        response.Close();
                        continue;
                    }
                }
                if (Path.GetFileName(filename) == "additem")
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        CustomAmmoCategoriesLog.Log.LogWrite("Запрос на добавление предмета\n");
                        System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
                        BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
                        CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
                        if (gameInstance == null)
                        {
                            jresp["error"] = "Не могу получить инстанс игры";
                            SendResponce(ref response, jresp);
                            continue;
                        }
                        BattleTech.SimGameState gameState = gameInstance.Simulation;
                        CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
                        if (gameState == null)
                        {
                            jresp["error"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
                            SendResponce(ref response, jresp);
                            continue;
                        }
                        if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN))
                        {
                            jresp["error"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
                        }
                        string data = reader.ReadToEnd();
                        CDefItem itm = JsonConvert.DeserializeObject<CDefItem>(data);
                        try
                        {
                            if (itm.type == BattleTech.ShopItemType.Mech)
                            {
                                gameState.CurSystem.SystemShop.ActiveInventory.Add(new BattleTech.ShopDefItem(itm.name, BattleTech.ShopItemType.Mech, 0.0f, itm.count, false, false, itm.price));
                                gameState.AddFunds(itm.price);
                            }
                            else
                            {
                                gameState.AddFromShopDefItem(new BattleTech.ShopDefItem(itm.name, itm.type, 0.0f, itm.count, false, false, itm.price));
                            }
                            jresp["success"] = "yes";
                        }
                        catch (Exception e)
                        {
                            jresp["error"] = e.ToString();
                        }

                        //gameState.PilotRoster.ElementAt(0).AddAbility("");
                        SendResponce(ref response, jresp);
                        continue;
                    }

                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Weapon_Laser_MediumLaser_2-Magna", BattleTech.ShopItemType.Weapon, 0.0f, 6, false, false, 80000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Weapon_SRM_SRM6_3-Valiant", BattleTech.ShopItemType.Weapon, 0.0f, 4, false, false, 140000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_TargetingTrackingSystem_Hartford_S2000", BattleTech.ShopItemType.Upgrade, 0.0f, 8, false, false, 2060000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_TargetingTrackingSystem_RCA_InstaTrac-XII", BattleTech.ShopItemType.Upgrade, 0.0f, 8, false, false, 2460000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_TargetingTrackingSystem_Kallon_Lock-On", BattleTech.ShopItemType.Upgrade, 0.0f, 8, false, false, 3080000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_Cockpit_Majesty_M_M_MagestrixAlpha", BattleTech.ShopItemType.Upgrade, 0.0f, 2, false, false, 1520000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_Cockpit_StarCorps_Dalban", BattleTech.ShopItemType.Upgrade, 0.0f, 6, false, false, 940000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_HeatSink_Generic_Double", BattleTech.ShopItemType.HeatSink, 0.0f, 20, false, false, 630000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_HeatSink_Generic_Thermal-Exchanger-I", BattleTech.ShopItemType.HeatSink, 0.0f, 20, false, false, 360000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_HeatSink_Generic_Thermal-Exchanger-II", BattleTech.ShopItemType.HeatSink, 0.0f, 20, false, false, 540000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Gear_HeatSink_Generic_Thermal-Exchanger-III", BattleTech.ShopItemType.HeatSink, 0.0f, 20, false, false, 720000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Weapon_Gauss_Gauss_2-M9", BattleTech.ShopItemType.Weapon, 0.0f, 15, false, false, 4440000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Weapon_PPC_PPCER_2-TiegartMagnum", BattleTech.ShopItemType.Weapon, 0.0f, 15, false, false, 2790000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Weapon_LRM_LRM20_3-Zeus", BattleTech.ShopItemType.Weapon, 0.0f, 15, false, false, 400000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Weapon_LRM_LRM10_3-Zeus", BattleTech.ShopItemType.Weapon, 0.0f, 10, false, false, 120000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Weapon_LRM_LRM5_3-Zeus", BattleTech.ShopItemType.Weapon, 0.0f, 20, false, false, 120000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("Ammo_AmmunitionBox_Generic_GAUSS", BattleTech.ShopItemType.AmmunitionBox, 0.0f, 15, false, false, 50000));
                    //gameState.CurSystem.SystemShop.ActiveInventory.Add(new BattleTech.ShopDefItem("mechdef_kingcrab_KGC-0000", BattleTech.ShopItemType.MechPart, 0.0f, 1, false, false, 1270000));
                    //gameState.CurSystem.SystemShop.ActiveInventory.Add(new BattleTech.ShopDefItem("mechdef_atlas_AS7-D", BattleTech.ShopItemType.MechPart, 0.0f, 1, false, false, 1300000));
                    //gameState.CurSystem.SystemShop.ActiveInventory.Add(new BattleTech.ShopDefItem("mechdef_highlander_HGN-733P", BattleTech.ShopItemType.MechPart, 0.0f, 1, false, false, 1120000));
                    //gameState.AddFromShopDefItem(new BattleTech.ShopDefItem("mechdef_atlas_AS7-D-HT", BattleTech.ShopItemType.MechPart, 0.0f, 2, false, false, 2060000));
                    //gameState.CurSystem.SystemShop.ActiveInventory.Add(new BattleTech.ShopDefItem("mechdef_atlas_AS7-D-HT", BattleTech.ShopItemType.MechPart, 0.0f, 1, false, false, 2060000));
                    //gameState.PilotRoster.ElementAt(0).AddAbility("");
                }
                if (Path.GetFileName(filename) == "test")
                {
                    System.Collections.Generic.Dictionary<string, string> jresp = new Dictionary<string, string>();
                    BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
                    if (gameInstance == null)
                    {
                        jresp["error"] = "Не могу получить инстанс игры";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    //gameInstance.DataManager.
                    foreach (var descr in gameInstance.DataManager.BaseDescriptionDefs)
                    {
                        jresp[descr.Key] = descr.Value.ToJSON();
                    }
                    SendResponce(ref response, jresp);
                    continue;
                }
                if (Path.GetFileName(filename) == "uppilots")
                {
                    CustomAmmoCategoriesLog.Log.LogWrite("Запрос на список пилотов\n");
                    Dictionary<string, Dictionary<string, string>> jresp = new Dictionary<string, Dictionary<string, string>>();
                    BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
                    if (gameInstance == null)
                    {
                        jresp["error"] = new Dictionary<string, string>();
                        jresp["error"]["string"] = "Не могу получить инстанс игры";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    BattleTech.SimGameState gameState = gameInstance.Simulation;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
                    if (gameState == null)
                    {
                        jresp["error"] = new Dictionary<string, string>();
                        jresp["error"]["string"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN))
                    {
                        jresp["error"] = new Dictionary<string, string>();
                        jresp["error"]["string"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    CustomAmmoCategoriesLog.Log.LogWrite("Информация о командире ...\n");
                    jresp[gameState.Commander.Callsign] = new Dictionary<string, string>();
                    jresp[gameState.Commander.Callsign]["name"] = gameState.Commander.FirstName;
                    CustomAmmoCategoriesLog.Log.LogWrite("...получена\n");
                    BattleTech.AbilityDef DefGu5 = gameState.DataManager.AbilityDefs.Get("AbilityDefGu5");
                    BattleTech.AbilityDef DefP5 = gameState.DataManager.AbilityDefs.Get("AbilityDefP5");
                    BattleTech.AbilityDef DefP8 = gameState.DataManager.AbilityDefs.Get("AbilityDefP8");
                    BattleTech.Ability Gu = new BattleTech.Ability(DefGu5);
                    BattleTech.Ability P5 = new BattleTech.Ability(DefP5);
                    BattleTech.Ability P8 = new BattleTech.Ability(DefP8);
                    gameState.Commander.Abilities.Add(Gu);
                    gameState.Commander.Abilities.Add(P5);
                    gameState.Commander.Abilities.Add(P8);
                    gameState.Commander.PassiveAbilities.Add(Gu);
                    gameState.Commander.PassiveAbilities.Add(P5);
                    gameState.Commander.PassiveAbilities.Add(P8);
                    foreach (var ability in gameState.Commander.Abilities)
                    {
                        jresp[gameState.Commander.Callsign][ability.Def.Id] = "1";
                    }
                    foreach (var pilot in gameState.PilotRoster)
                    {
                        jresp[pilot.Callsign] = new Dictionary<string, string>();
                        jresp[pilot.Callsign]["name"] = pilot.FirstName;
                        Gu = new BattleTech.Ability(DefGu5);
                        P5 = new BattleTech.Ability(DefP5);
                        P8 = new BattleTech.Ability(DefP8);
                        pilot.Abilities.Add(Gu);
                        pilot.Abilities.Add(P5);
                        pilot.Abilities.Add(P8);
                        pilot.PassiveAbilities.Add(Gu);
                        pilot.PassiveAbilities.Add(P5);
                        pilot.PassiveAbilities.Add(P8);
                        //pilot.StatCollection.AddStatistic
                        foreach (var ability in pilot.Abilities)
                        {
                            jresp[pilot.Callsign][ability.Def.Id] = "1";
                        }
                    }
                    CustomAmmoCategoriesLog.Log.LogWrite("формирование ответа\n");
                    SendResponce(ref response, jresp);
                    continue;
                }
                if (Path.GetFileName(filename) == "listpilots")
                {
                    CustomAmmoCategoriesLog.Log.LogWrite("Запрос на список пилотов\n");
                    Dictionary<string, Dictionary<string, string>> jresp = new Dictionary<string, Dictionary<string, string>>();
                    BattleTech.GameInstance gameInstance = BattleTech.UnityGameInstance.BattleTechGame;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен gameInstance\n");
                    if (gameInstance == null)
                    {
                        jresp["error"] = new Dictionary<string, string>();
                        jresp["error"]["string"] = "Не могу получить инстанс игры";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    BattleTech.SimGameState gameState = gameInstance.Simulation;
                    CustomAmmoCategoriesLog.Log.LogWrite("Получен gameState\n");
                    if (gameState == null)
                    {
                        jresp["error"] = new Dictionary<string, string>();
                        jresp["error"]["string"] = "Не могу получить состояние симулятора. Скорее всего не загружено сохранение";
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    if ((gameState.SimGameMode != BattleTech.SimGameState.SimGameType.CAREER) && (gameState.SimGameMode != BattleTech.SimGameState.SimGameType.KAMEA_CAMPAIGN))
                    {
                        jresp["error"] = new Dictionary<string, string>();
                        jresp["error"]["string"] = "Неправильный режим компании:" + gameState.SimGameMode.ToString();
                        SendResponce(ref response, jresp);
                        continue;
                    }
                    CustomAmmoCategoriesLog.Log.LogWrite("Информация о командире ...\n");
                    jresp[gameState.Commander.Callsign] = new Dictionary<string, string>();
                    jresp[gameState.Commander.Callsign]["name"] = gameState.Commander.FirstName;
                    CustomAmmoCategoriesLog.Log.LogWrite("...получена\n");

                    foreach (var ability in gameState.Commander.Abilities)
                    {
                        jresp[gameState.Commander.Callsign][ability.Def.Id] = "1";
                    }
                    foreach (var pilot in gameState.PilotRoster)
                    {
                        jresp[pilot.Callsign] = new Dictionary<string, string>();
                        jresp[pilot.Callsign]["name"] = pilot.FirstName;
                        foreach (var ability in pilot.Abilities)
                        {
                            jresp[pilot.Callsign][ability.Def.Id] = "1";
                        }
                    }
                    CustomAmmoCategoriesLog.Log.LogWrite("формирование ответа\n");
                    SendResponce(ref response, jresp);
                    continue;
                }
                CustomAmmoCategoriesLog.Log.LogWrite("Неизвестный запрос\n");
                SendError(ref response, 400, "Не могу найти " + filename);
            }
        }
    }
}
