using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace ValiModern.Services
{
    public class ColorOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ColorCode { get; set; }
    }
    public class SizeOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    internal class PaletteStore<T>
    {
        public int LastId { get; set; }
        public List<T> Items { get; set; } = new List<T>();
    }

    public static class PaletteService
    {
        private static readonly object _lock = new object();
        private static string Root => HttpContext.Current.Server.MapPath("~/App_Data/Palettes");
        private static string ColorsPath => Path.Combine(Root, "colors.json");
        private static string SizesPath => Path.Combine(Root, "sizes.json");

        private static PaletteStore<T> ReadStore<T>(string path)
        {
            if (!Directory.Exists(Root)) Directory.CreateDirectory(Root);
            if (!File.Exists(path))
            {
                return new PaletteStore<T> { LastId = 0, Items = new List<T>() };
            }
            var json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json)) return new PaletteStore<T>();
            return JsonConvert.DeserializeObject<PaletteStore<T>>(json) ?? new PaletteStore<T>();
        }
        private static void WriteStore<T>(string path, PaletteStore<T> data)
        {
            if (!Directory.Exists(Root)) Directory.CreateDirectory(Root);
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public static List<ColorOption> GetColors()
        {
            lock (_lock)
            {
                var store = ReadStore<ColorOption>(ColorsPath);
                return store.Items.OrderBy(i => i.Id).ToList();
            }
        }
        public static ColorOption AddColor(string name, string code)
        {
            lock (_lock)
            {
                var store = ReadStore<ColorOption>(ColorsPath);
                var exists = store.Items.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                if (exists) throw new InvalidOperationException("Color already exists");
                store.LastId++;
                var item = new ColorOption { Id = store.LastId, Name = name, ColorCode = code };
                store.Items.Add(item);
                WriteStore(ColorsPath, store);
                return item;
            }
        }
        public static void UpdateColor(int id, string name, string code)
        {
            lock (_lock)
            {
                var store = ReadStore<ColorOption>(ColorsPath);
                var item = store.Items.FirstOrDefault(x => x.Id == id);
                if (item == null) throw new InvalidOperationException("Not found");
                if (!string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase) && store.Items.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("Color already exists");
                item.Name = name; item.ColorCode = code;
                WriteStore(ColorsPath, store);
            }
        }
        public static void DeleteColor(int id)
        {
            lock (_lock)
            {
                var store = ReadStore<ColorOption>(ColorsPath);
                store.Items = store.Items.Where(x => x.Id != id).ToList();
                WriteStore(ColorsPath, store);
            }
        }

        public static List<SizeOption> GetSizes()
        {
            lock (_lock)
            {
                var store = ReadStore<SizeOption>(SizesPath);
                return store.Items.OrderBy(i => i.Id).ToList();
            }
        }
        public static SizeOption AddSize(string name)
        {
            lock (_lock)
            {
                var store = ReadStore<SizeOption>(SizesPath);
                var exists = store.Items.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                if (exists) throw new InvalidOperationException("Size already exists");
                store.LastId++;
                var item = new SizeOption { Id = store.LastId, Name = name };
                store.Items.Add(item);
                WriteStore(SizesPath, store);
                return item;
            }
        }
        public static void UpdateSize(int id, string name)
        {
            lock (_lock)
            {
                var store = ReadStore<SizeOption>(SizesPath);
                var item = store.Items.FirstOrDefault(x => x.Id == id);
                if (item == null) throw new InvalidOperationException("Not found");
                if (!string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase) && store.Items.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase)))
                    throw new InvalidOperationException("Size already exists");
                item.Name = name;
                WriteStore(SizesPath, store);
            }
        }
        public static void DeleteSize(int id)
        {
            lock (_lock)
            {
                var store = ReadStore<SizeOption>(SizesPath);
                store.Items = store.Items.Where(x => x.Id != id).ToList();
                WriteStore(SizesPath, store);
            }
        }
    }
}
