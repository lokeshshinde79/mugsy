using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.Text.RegularExpressions;

namespace NovelProjects.Web
{
  #region Custom RequiredFieldValidator for a CheckBoxList
  public class RequiredFieldValidatorForCheckBoxLists : System.Web.UI.WebControls.BaseValidator
  {
    private ListControl _listctrl;

    public RequiredFieldValidatorForCheckBoxLists()
    {
      base.EnableClientScript = false;
    }

    protected override bool ControlPropertiesValid()
    {
      Control ctrl = FindControl(ControlToValidate);

      if (ctrl != null)
      {
        _listctrl = (ListControl)ctrl;
        return (_listctrl != null);
      }
      else
        return false;  // raise exception
    }

    protected override bool EvaluateIsValid()
    {
      return _listctrl.SelectedIndex != -1;
    }
  }
  #endregion

  #region Custom RequiredFieldValidator for a CheckBox
  public class RequiredFieldValidatorForCheckBox : System.Web.UI.WebControls.BaseValidator
  {
    private CheckBox chkbox;

    public RequiredFieldValidatorForCheckBox()
    {
      base.EnableClientScript = false;
    }

    protected override bool ControlPropertiesValid()
    {
      Control ctrl = FindControl(ControlToValidate);

      if (ctrl != null)
      {
        chkbox = (CheckBox)ctrl;
        return (chkbox != null);
      }
      else
        return false;  // raise exception
    }

    protected override bool EvaluateIsValid()
    {
      return chkbox.Checked;
    }
  }
  #endregion

  #region New Credit Card Validator
  public class CardValidator : System.Web.UI.WebControls.BaseValidator
  {
    public enum CreditCardType
    {
      MasterCard = 1,
      VISA = 2,
      Amex = 3,
      DinersClub = 4,
      enRoute = 5,
      Discover = 6,
      JCB = 7
    }
    
    
    private string _CreditCardTypesDropDown;
    private CreditCardType _CardType;
    private string _CardNumber;
    private System.Web.UI.WebControls.TextBox _creditCardTextBox;
    private System.Web.UI.WebControls.DropDownList _creditCardTypeDropDownList;
    
    public string CreditCardTypesDropDown
    {
      get
      {
        return _CreditCardTypesDropDown;
      }
      set
      {
        _CreditCardTypesDropDown = value;
      }
    }
    
    public CardValidator() { } // static only

    protected override bool ControlPropertiesValid()
    {
      Control cardNum = FindControl(ControlToValidate);
      Control cardTypes = FindControl(CreditCardTypesDropDown);

      if (null != cardNum)
      {
        if (cardNum is System.Web.UI.WebControls.TextBox && cardTypes is System.Web.UI.WebControls.DropDownList)	// ensure its a text box
        {
          _creditCardTextBox = (System.Web.UI.WebControls.TextBox)cardNum;	// set the member variable
          _creditCardTypeDropDownList = (System.Web.UI.WebControls.DropDownList)cardTypes;
          //return (null != _creditCardTextBox);		// check that it's been set ok

          return (_creditCardTextBox != null && _creditCardTypeDropDownList != null);

          //if (_creditCardTextBox != null && _creditCardTypeDropDownList != null)
          //{
            
          //}
        }
        else
          return false;
      }
      else
        return false;
    }

    protected override bool EvaluateIsValid()
    {
      Int16 cardtypval;

      if (Int16.TryParse(_creditCardTypeDropDownList.SelectedValue, out cardtypval))
      {
        _CardType = (CreditCardType)cardtypval;
        _CardNumber = _creditCardTextBox.Text;

        return ValidateCard(_CardType, _CardNumber);
      }

      return false;
    }

    public static bool ValidateCard(CreditCardType cardType, string cardNumber)
    {
      byte[] number = new byte[16]; // number to validate

      // Remove non-digits
      int len = 0;
      for (int i = 0; i < cardNumber.Length; i++)
      {
        if (char.IsDigit(cardNumber, i))
        {
          if (len == 16) return false; // number has too many digits
          number[len++] = byte.Parse(cardNumber[i].ToString());
        }
      }

      // Validate based on card type, first if tests length, second tests prefix
      switch (cardType)
      {
        case CreditCardType.MasterCard:
          if (len != 16)
            return false;
          if (number[0] != 5 || number[1] == 0 || number[1] > 5)
            return false;
          break;

        //case CreditCardType.BankCard:
        //  if (len != 16)
        //    return false;
        //  if (number[0] != 5 || number[1] != 6 || number[2] > 1)
        //    return false;
        //  break;

        case CreditCardType.VISA:
          if (len != 16 && len != 13)
            return false;
          if (number[0] != 4)
            return false;
          break;

        case CreditCardType.Amex:
          if (len != 15)
            return false;
          if (number[0] != 3 || (number[1] != 4 && number[1] != 7))
            return false;
          break;

        case CreditCardType.Discover:
          if (len != 16)
            return false;
          if (number[0] != 6 || number[1] != 0 || number[2] != 1 || number[3] != 1)
            return false;
          break;

        case CreditCardType.DinersClub:
          if (len != 14)
            return false;
          if (number[0] != 3 || (number[1] != 0 && number[1] != 6 && number[1] != 8)
             || number[1] == 0 && number[2] > 5)
            return false;
          break;

        case CreditCardType.JCB:
          if (len != 16 && len != 15)
            return false;
          if (number[0] != 3 || number[1] != 5)
            return false;
          break;
      }

      // Use Luhn Algorithm to validate
      int sum = 0;
      for (int i = len - 1; i >= 0; i--)
      {
        if (i % 2 == len % 2)
        {
          int n = number[i] * 2;
          sum += (n / 10) + (n % 10);
        }
        else
          sum += number[i];
      }
      return (sum % 10 == 0);
    }
  }
  #endregion

  #region Old Credit Card Validator
  /// <summary>
  /// Summary description for CreditCardValidator.
  /// </summary>
  [Flags, Serializable]
  public enum CardType
  {
    MasterCard = 0x0001,
    VISA = 0x0002,
    Amex = 0x0004,
    DinersClub = 0x0008,
    enRoute = 0x0010,
    Discover = 0x0020,
    JCB = 0x0040,
    Unknown = 0x0080,
    All = CardType.Amex | CardType.DinersClub | CardType.Discover | CardType.Discover |
      CardType.enRoute | CardType.JCB | CardType.MasterCard | CardType.VISA
  }

  [Obsolete("This is a deprecated class. Use Novelprojects.Web.CardValidator")]
  /// <summary>
  /// This is a deprecated class. Use Novelprojects.Web.CardValidator.
  /// </summary>
  public class CreditCardValidator : System.Web.UI.WebControls.BaseValidator
  {
    private CardType _cardTypes;
    private System.Web.UI.WebControls.TextBox _creditCardTextBox;
    private bool _validateCardType;

    public CreditCardValidator()
    {
      _validateCardType = true;
      _cardTypes = CardType.All | CardType.Unknown;	// Accept everything
    }

    protected override bool ControlPropertiesValid()
    {
      Control ctrl = FindControl(ControlToValidate);

      if (null != ctrl)
      {
        if (ctrl is System.Web.UI.WebControls.TextBox)	// ensure its a text box
        {
          _creditCardTextBox = (System.Web.UI.WebControls.TextBox)ctrl;	// set the member variable
          return (null != _creditCardTextBox);		// check that it's been set ok
        }
        else
          return false;
      }
      else
        return false;
    }

    protected override bool EvaluateIsValid()
    {
      if (_validateCardType)	// should the length be validated also?
      {
        if (IsValidCardType(_creditCardTextBox.Text))
          return ValidateCardNumber(_creditCardTextBox.Text);
        else
          return false;		// Invalid length
      }
      else
        return ValidateCardNumber(_creditCardTextBox.Text);
    }

    public bool IsValidCardType(string cardNumber)
    {
      // AMEX -- 34 or 37 -- 15 length
      if ((Regex.IsMatch(cardNumber, "^(34|37)")) && ((_cardTypes & CardType.Amex) != 0))
        return (15 == cardNumber.Length);

        // MasterCard -- 51 through 55 -- 16 length
      else if ((Regex.IsMatch(cardNumber, "^(51|52|53|54|55)")) && ((_cardTypes & CardType.MasterCard) != 0))
        return (16 == cardNumber.Length);

        // VISA -- 4 -- 13 and 16 length
      else if ((Regex.IsMatch(cardNumber, "^(4)")) && ((_cardTypes & CardType.VISA) != 0))
        return (13 == cardNumber.Length || 16 == cardNumber.Length);

        // Discover -- 6011 -- 16 length
      else if ((Regex.IsMatch(cardNumber, "^(6011)")) && ((_cardTypes & CardType.Discover) != 0))
        return (16 == cardNumber.Length);

      else
      {
        if ((_cardTypes & CardType.Unknown) != 0)
          return true;
        else
          return false;
      }
    }

    private static bool ValidateCardNumber(string cardNumber)
    {
      try
      {
        System.Collections.ArrayList CheckNumbers = new ArrayList();

        int CardLength = cardNumber.Length;

        for (int i = CardLength - 2; i >= 0; i = i - 2)
        {
          CheckNumbers.Add(Int32.Parse(cardNumber[i].ToString()) * 2);
        }

        int CheckSum = 0;	// Will hold the total sum of all checksum digits

        for (int iCount = 0; iCount <= CheckNumbers.Count - 1; iCount++)
        {
          int _count = 0;	// will hold the sum of the digits

          if ((int)CheckNumbers[iCount] > 9)
          {
            int _numLength = ((int)CheckNumbers[iCount]).ToString().Length;
            // add count to each digit
            for (int x = 0; x < _numLength; x++)
            {
              _count = _count + Int32.Parse(((int)CheckNumbers[iCount]).ToString()[x].ToString());
            }
          }
          else
          {
            _count = (int)CheckNumbers[iCount];	// single digit, just add it by itself
          }

          CheckSum = CheckSum + _count;	// add sum to the total sum
        }

        int OriginalSum = 0;
        for (int y = CardLength - 1; y >= 0; y = y - 2)
        {
          OriginalSum = OriginalSum + Int32.Parse(cardNumber[y].ToString());
        }

        return (((OriginalSum + CheckSum) % 10) == 0);
      }
      catch
      {
        return false;
      }
    }

    public bool ValidateCardType
    {
      get
      {
        return _validateCardType;
      }
      set
      {
        _validateCardType = value;
      }
    }

    public string AcceptedCardTypes
    {
      get
      {
        return _cardTypes.ToString();
      }
      set
      {
        _cardTypes = (NovelProjects.Web.CardType)Enum.Parse(typeof(NovelProjects.Web.CardType), value, false);
      }
    }
  }
  #endregion
}