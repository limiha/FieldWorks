// Copyright (c) 2007-2020 SIL International
// This software is licensed under the LGPL, version 2.1 or later
// (http://www.gnu.org/licenses/lgpl-2.1.html)

namespace SIL.FieldWorks.Common.FwUtils
{
	/// <summary>
	/// Class providing some general static methods for dealing with measurements.
	/// </summary>
	public class MeasurementUtils
	{
		private static readonly double kMpPerCm = 72000.0 / 2.54;
		private static readonly double kMpPerInch = 72000.0;
		private static readonly double kMpPerMm = 72000.0 / 25.4;
		private static readonly double kMpPerPt = 1000.0;

		/// <summary>
		/// Extracts the measurement from a formatted string (i.e., possibly having units
		/// specified), converting it to millipoints.
		/// </summary>
		/// <param name="str">The source string</param>
		/// <param name="defaultUnits">The default units to use if none are specified in the
		/// string.</param>
		/// <param name="defaultValue">The default value to return if the string cannot be
		/// parsed.</param>
		/// <returns>
		/// The number of millipoints represented by the measurement specified in the
		/// string
		/// </returns>
		public static double ExtractMeasurementInMillipoints(string str, MsrSysType defaultUnits, double defaultValue)
		{
			try
			{
				int ichMsr;
				double mpPerUnitFactor;

				// if number is in inches (?.?? in or ??.?")
				if (((ichMsr = str.IndexOf(FwUtilsStrings.kstidIn.Trim())) > 0) || ((ichMsr = str.IndexOf(FwUtilsStrings.kstidInches.Trim())) > 0))
				{
					mpPerUnitFactor = kMpPerInch;
				}
				else if ((ichMsr = str.IndexOf(FwUtilsStrings.kstidMm.Trim())) > 0)
				{
					mpPerUnitFactor = kMpPerMm;
				}
				else if ((ichMsr = str.IndexOf(FwUtilsStrings.kstidCm.Trim())) > 0)
				{
					mpPerUnitFactor = kMpPerCm;
				}
				else if ((ichMsr = str.IndexOf(FwUtilsStrings.kstidPt.Trim())) > 0)
				{
					mpPerUnitFactor = kMpPerPt;
				}
				else
				{
					mpPerUnitFactor = GetMpPerUnitFactor(defaultUnits);
				}
				if (ichMsr > 0)
				{
					str = str.Substring(0, ichMsr);
				}
				return double.Parse(str.Trim()) * mpPerUnitFactor;
			}
			catch
			{
				// If an error occurs, return the default value supplied by the caller.
				return defaultValue;
			}
		}

		/// <summary>
		/// Gets the millipoint per unit factor for the given measure type.
		/// </summary>
		/// <param name="msrType">The measure type.</param>
		/// <returns>Number of millipoints per given unit</returns>
		public static double GetMpPerUnitFactor(MsrSysType msrType)
		{
			switch (msrType)
			{
				case MsrSysType.Cm: return kMpPerCm;
				case MsrSysType.Inch: return kMpPerInch;
				case MsrSysType.Mm: return kMpPerMm;
				case MsrSysType.Point: return kMpPerPt;
			}
			// The above handles all MsrSysType enums options, so theory has it, we can't get here from there.
			return kMpPerPt;
		}

		/// <summary>
		/// Gets the get measurement units abbreviation for the given measurement type.
		/// </summary>
		/// <param name="msrType">The measure type.</param>
		/// <returns>The (localizable) abbreviation which can be appended to a numeric value to
		/// express a measurement</returns>
		public static string GetMeasurementUnitsAbbrev(MsrSysType msrType)
		{
			switch (msrType)
			{
				case MsrSysType.Cm: return FwUtilsStrings.kstidCm;
				case MsrSysType.Inch: return FwUtilsStrings.kstidInches;
				case MsrSysType.Mm: return FwUtilsStrings.kstidMm;
				case MsrSysType.Point: return FwUtilsStrings.kstidPt;
			}
			// The above handles all MsrSysType enums options, so theory has it, we can't get here from there.
			return FwUtilsStrings.kstidPt;
		}

		/// <summary>
		/// Formats the measurement to a maximum of two decimal places of precision.
		/// </summary>
		/// <param name="mptValue">The value (in millipoints).</param>
		/// <param name="msrType">The measure type.</param>
		/// <returns>A formatted (localized) string representing the given number of millipoints
		/// in the requested units</returns>
		public static string FormatMeasurement(double mptValue, MsrSysType msrType)
		{
			return FormatMeasurement(mptValue, msrType, false);
		}

		/// <summary>
		/// Formats the measurement.
		/// </summary>
		/// <param name="mptValue">The value (in millipoints).</param>
		/// <param name="msrType">The measure type.</param>
		/// <param name="fUseVariablePrecision">Flag indicating whether to use variable
		/// precision (depending on units) for formatting the value. Inches use two decimal
		/// places, centimeters one, and all other units display to the nearest integer.</param>
		/// <returns>A formatted (localized) string representing the given number of millipoints
		/// in the requested units</returns>
		static public string FormatMeasurement(double mptValue, MsrSysType msrType, bool fUseVariablePrecision)
		{
			var sFormat = "##########0.##";
			if (fUseVariablePrecision)
			{
				switch (msrType)
				{
					case MsrSysType.Inch: break;
					case MsrSysType.Cm: sFormat = "##########0.#"; break;
					default: sFormat = "##########0"; break;
				}
			}

			return FormatMeasurement(mptValue, msrType, sFormat);
		}

		/// <summary>
		/// Formats the measurement.
		/// </summary>
		/// <param name="mptValue">The value (in millipoints).</param>
		/// <param name="msrType">The measure type.</param>
		/// <param name="sFormat">The format string to use</param>
		/// <returns>A formatted (localized) string representing the given number of millipoints
		/// in the requested units</returns>
		public static string FormatMeasurement(double mptValue, MsrSysType msrType, string sFormat)
		{
			return (mptValue / GetMpPerUnitFactor(msrType)).ToString(sFormat) + GetMeasurementUnitsAbbrev(msrType);
		}
	}
}