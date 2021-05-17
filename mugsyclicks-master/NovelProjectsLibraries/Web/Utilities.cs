using System;
using System.Text.RegularExpressions;

namespace NovelProjects.Web
{
	public class Utilities
	{
		public static Boolean IsEmail(String Email)
		{
			string strRegex = @"^(([a-zA-Z0-9_\-\+/\^]+)([\.]?)([a-zA-Z0-9_\-\+/\^]+))+@((\[[0-9]{1,3}" +
					@"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
					@".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
			Regex re = new Regex(strRegex);
			return re.IsMatch(Email);
		}

		public static Boolean GuidTryParse(String s, out Guid g)
		{
			g = Guid.Empty;

			if (String.IsNullOrEmpty(s))
				return false;

			s = s.Replace("-", "").Replace("{", "").Replace("}", "");

			Regex r = new Regex("[0-9A-Fa-f]{32}");

			if (!r.IsMatch(s))
				return false;

			g = new Guid(s);

			return true;
		}

		public static Decimal TruncateFunction(Decimal number, int digits)
		{
			Decimal stepper = (Decimal)(Math.Pow(10.0, (double)digits));
			int temp = (int)(stepper * number);
			return (Decimal)temp / stepper;
		}

		public static Double TruncateFunction(Double number, int digits)
		{
			Double stepper = (Double)(Math.Pow(10.0, (double)digits));
			int temp = (int)(stepper * number);
			return (Double)temp / stepper;
		}
	}
}
