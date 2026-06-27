using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using MindArcade.Core.Llm;
using MindArcade.Core.Firebase;

namespace MindArcade.Core.Utils
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct OpenFileName
    {
        public int structSize;
        public IntPtr dlgOwner;
        public IntPtr instance;
        public string filter;
        public string customFilter;
        public int maxCustFilter;
        public int filterIndex;
        public string file;
        public int maxFile;
        public string fileTitle;
        public int maxFileTitle;
        public string initialDir;
        public string title;
        public int flags;
        public short fileOffset;
        public short fileExtension;
        public string defExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int dwReserved;
        public int flagsEx;
    }

    public static class ReportExporter
    {
        [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetSaveFileName(ref OpenFileName ofn);

        private static string SaveFileDialog(string defaultFileName, string filterText, string extension)
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            try
            {
                OpenFileName ofn = new OpenFileName();
                ofn.structSize = Marshal.SizeOf(ofn);
                ofn.filter = filterText;
                
                // Max length of 512 characters
                ofn.file = new string(new char[512]);
                if (!string.IsNullOrEmpty(defaultFileName))
                {
                    defaultFileName.CopyTo(0, ofn.file.ToCharArray(), 0, Math.Min(defaultFileName.Length, 511));
                }
                ofn.maxFile = ofn.file.Length;
                ofn.fileTitle = new string(new char[64]);
                ofn.maxFileTitle = ofn.fileTitle.Length;
                ofn.initialDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                ofn.title = "Raporu Kaydet";
                ofn.defExt = extension;
                ofn.flags = 0x00000002 | 0x00000008; // OFN_OVERWRITEPROMPT | OFN_NOCHANGEDIR

                if (GetSaveFileName(ref ofn))
                {
                    return ofn.file.Split('\0')[0]; // Clean trailing nulls
                }
                return null; // Cancelled
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[FileSaveDialog] Win32 dialog failed, using fallback. Error: {ex.Message}");
            }
#endif
            // Fallback to Desktop
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), defaultFileName);
        }

        public static string ExportReport(string patientId, string name, PlayerProfile profile, List<FirebaseSessionData> sessions)
        {
            if (profile == null) profile = new PlayerProfile();

            // Save File Dialog'u aç
            string defaultFilename = "Rapor_" + patientId + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".html";
            string filter = "HTML Dosyası (*.html)\0*.html\0Tüm Dosyalar (*.*)\0*.*\0";
            string path = SaveFileDialog(defaultFilename, filter, "html");

            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("[ReportExporter] Rapor kaydetme kullanıcı tarafından iptal edildi.");
                return null;
            }

            StringBuilder sb = new StringBuilder();

            // HTML Başlangıcı ve CSS Stilleri
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html lang='tr'>");
            sb.AppendLine("<head>");
            sb.AppendLine("    <meta charset='UTF-8'>");
            sb.AppendLine("    <title>Bilişsel Performans Raporu - " + name + "</title>");
            sb.AppendLine("    <style>");
            sb.AppendLine("        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 40px; color: #333; background-color: #f9f9f9; }");
            sb.AppendLine("        .container { max-width: 900px; margin: 0 auto; background: #fff; padding: 30px; border-radius: 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.05); }");
            sb.AppendLine("        .header { display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #5d5baf; padding-bottom: 20px; margin-bottom: 30px; }");
            sb.AppendLine("        .header h1 { color: #5d5baf; margin: 0; font-size: 28px; }");
            sb.AppendLine("        .patient-info { margin-bottom: 30px; background: #f0eff9; padding: 20px; border-radius: 6px; border-left: 5px solid #5d5baf; }");
            sb.AppendLine("        .patient-info table { width: 100%; border-collapse: collapse; }");
            sb.AppendLine("        .patient-info td { padding: 6px 12px; font-size: 16px; }");
            sb.AppendLine("        .patient-info td.label { font-weight: bold; color: #555; width: 150px; }");
            sb.AppendLine("        h2 { color: #333; border-bottom: 1px solid #ddd; padding-bottom: 8px; margin-top: 40px; font-size: 20px; }");
            sb.AppendLine("        table.data-table { width: 100%; border-collapse: collapse; margin-top: 15px; background: #fff; }");
            sb.AppendLine("        table.data-table th { background-color: #5d5baf; color: white; padding: 12px; text-align: left; font-size: 14px; }");
            sb.AppendLine("        table.data-table td { padding: 12px; border-bottom: 1px solid #eee; font-size: 14px; }");
            sb.AppendLine("        table.data-table tr:nth-child(even) { background-color: #f8f8fc; }");
            sb.AppendLine("        .btn-print { background-color: #4caf50; color: white; border: none; padding: 10px 20px; font-size: 16px; border-radius: 4px; cursor: pointer; font-weight: bold; transition: background 0.2s; }");
            sb.AppendLine("        .btn-print:hover { background-color: #45a049; }");
            sb.AppendLine("        @media print {");
            sb.AppendLine("            body { background: #fff; margin: 0; }");
            sb.AppendLine("            .container { box-shadow: none; padding: 0; }");
            sb.AppendLine("            .btn-print { display: none; }");
            sb.AppendLine("        }");
            sb.AppendLine("    </style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            sb.AppendLine("<div class='container'>");
            
            // Header Bölümü
            sb.AppendLine("    <div class='header'>");
            sb.AppendLine("        <div>");
            sb.AppendLine("            <h1>Bilişsel Performans Raporu</h1>");
            sb.AppendLine("            <p style='color: #666; margin: 5px 0 0 0;'>MindArcade Klinik Değerlendirme Sistemi</p>");
            sb.AppendLine("        </div>");
            sb.AppendLine("        <button class='btn-print' onclick='window.print()'>Yazdır / PDF Kaydet</button>");
            sb.AppendLine("    </div>");

            // Hasta Bilgileri
            sb.AppendLine("    <div class='patient-info'>");
            sb.AppendLine("        <table>");
            sb.AppendLine("            <tr><td class='label'>Hasta Adı:</td><td>" + name + "</td><td class='label'>Rapor Tarihi:</td><td>" + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</td></tr>");
            sb.AppendLine("            <tr><td class='label'>Hasta ID:</td><td>" + patientId + "</td><td class='label'></td><td></td></tr>");
            sb.AppendLine("        </table>");
            sb.AppendLine("    </div>");

            // Kümülatif İstatistikler
            sb.AppendLine("    <h2>Oyun Bazlı Kümülatif Performans Özeti</h2>");
            sb.AppendLine("    <table class='data-table'>");
            sb.AppendLine("        <thead>");
            sb.AppendLine("            <tr>");
            sb.AppendLine("                <th>Oyun Adı</th>");
            sb.AppendLine("                <th>Oynanma Sayısı</th>");
            sb.AppendLine("                <th>Ortalama Doğruluk</th>");
            sb.AppendLine("                <th>En Yüksek Skor</th>");
            sb.AppendLine("                <th>Ulaşılan Seviye</th>");
            sb.AppendLine("                <th>Ort. Bitirme Süresi (sn)</th>");
            sb.AppendLine("                <th>Ort. Hata Sayısı</th>");
            sb.AppendLine("            </tr>");
            sb.AppendLine("        </thead>");
            sb.AppendLine("        <tbody>");

            AppendGameStatsRow(sb, "MemoryMatch (Görsel Bellek)", profile.memoryMatchStats);
            AppendGameStatsRow(sb, "FocusRunner (Seçici Dikkat)", profile.focusRunnerStats);
            AppendGameStatsRow(sb, "SequenceCubes (Ardışıl Hafıza)", profile.sequenceCubesStats);

            sb.AppendLine("        </tbody>");
            sb.AppendLine("    </table>");

            sb.AppendLine("</div>"); // container
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            try
            {
                File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
                Debug.Log($"[ReportExporter] Rapor başarıyla kaydedildi: {path}");
                return path;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReportExporter] Rapor yazılırken hata: {ex.Message}");
                return null;
            }
        }

        private static void AppendGameStatsRow(StringBuilder sb, string gameName, GameStats stats)
        {
            if (stats == null) stats = new GameStats();

            sb.AppendLine("            <tr>");
            sb.AppendLine("                <td style='font-weight: bold;'>" + gameName + "</td>");
            sb.AppendLine("                <td>" + stats.totalGamesPlayed + "</td>");
            sb.AppendLine("                <td>%" + Mathf.RoundToInt(stats.averageAccuracy * 100) + "</td>");
            sb.AppendLine("                <td>" + stats.highestScore + "</td>");
            sb.AppendLine("                <td>" + stats.highestLevel + "</td>");
            sb.AppendLine("                <td>" + stats.averageCompletionTime.ToString("F1") + "</td>");
            sb.AppendLine("                <td>" + stats.averageMistakes.ToString("F1") + "</td>");
            sb.AppendLine("            </tr>");
        }
    }
}
