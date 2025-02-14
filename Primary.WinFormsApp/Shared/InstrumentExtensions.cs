﻿using Primary.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Primary.WinFormsApp
{
    public static class InstrumentExtensions
    {
        public static IEnumerable<Instrument> FilterByTicker(this IEnumerable<Instrument> instruments, string ticker)
        {
            var tickerSymbols = ticker.GetAllMervalSymbols();
            return instruments.Where(x => tickerSymbols.Contains(x.Symbol));
        }

        public static bool IsPesos(this InstrumentDetail instrumentDetail)
        {
            return string.Equals(instrumentDetail.Currency, "ARS", System.StringComparison.InvariantCultureIgnoreCase);
        }

        public static decimal GetIncrement(this InstrumentDetail instrumentDetail)
        {
            return 1m / Convert.ToDecimal("1".PadRight(instrumentDetail.InstrumentPricePrecision + 1, '0'));
        }

        public static bool IsCEDEAR(this InstrumentDetail instrumentDetail)
        {
            var exists = Properties.Settings.Default.AccionesCEDEARs.Cast<string>().Any(x => instrumentDetail.InstrumentId.Symbol.StartsWith($"{Instrument.MervalPrefix}{x}"));
            return exists;
        }

        public static bool IsLetra(this InstrumentDetail instrumentDetail)
        {
            var exists = Properties.Settings.Default.Letras.Cast<string>().Any(x => instrumentDetail.InstrumentId.Symbol.StartsWith($"{Instrument.MervalPrefix}{x}"));
            return exists;
        }

        public static decimal GetDerechosDeMercado(this InstrumentDetail instrumentDetail)
        {
            // https://www.byma.com.ar/que-es-byma/derechos-membresias-2/
            if (instrumentDetail.IsCEDEAR())
            {
                return (Properties.Settings.Default.Comision + Properties.Settings.Default.DerechoMercadoAccionCEDEAR) / 100m;
            }
            else if (instrumentDetail.IsLetra())
            {
                return (Properties.Settings.Default.Comision + Properties.Settings.Default.DerechoMercadoLetra) / 100m;
            }
            else
            {
                return (Properties.Settings.Default.Comision + Properties.Settings.Default.DerechoMercado) / 100m;
            }
        }

        public static decimal CalculateComisionDerechosMercado(this InstrumentDetail instrumentDetail, decimal amountInPesos)
        {
            var comision = instrumentDetail.GetDerechosDeMercado();
            return amountInPesos * comision;
        }

        public static string FormatCurrency(this InstrumentDetail instrumentDetail, decimal amount)
        {
            if (instrumentDetail.IsPesos())
            {
                return amount.ToCurrency();
            }
            else {
                return amount.ToUSD();
            }
        }

        public static string FormatPrice(this InstrumentDetail instrumentDetail, decimal value)
        {
            var decimalPlacesFormat = string.Empty.PadRight(instrumentDetail.InstrumentPricePrecision, '0');
            var format = $"#,##0.{decimalPlacesFormat}";

            return value.ToString(format);
        }

        public static int CalculateSettlementDays(this InstrumentDetail buy, InstrumentDetail sell, int diasLiq24H, int diasLiq48H)
        {
            int buyDiasLiq = buy.GetSettlementDays(diasLiq24H, diasLiq48H);
            int sellDiasLiq = sell.GetSettlementDays(diasLiq24H, diasLiq48H);

            var days = buyDiasLiq - sellDiasLiq;

            return days;
        }
    }
}
