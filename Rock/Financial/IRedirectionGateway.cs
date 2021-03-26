using System;
using System.Collections.Generic;

namespace Rock.Financial
{
    public class RedirectionGatewayField
    {
        public string Key { get; set; }
        public string DisplayName { get; set; }
        public string CurrentValue { get; set; }

        public Func<IEnumerable<string>, IEnumerable<KeyValuePair<string, string>>> GetItems;

        public RedirectionGatewayField ParentGatewayField { get; set; }

        public IEnumerable<KeyValuePair<string, string>> GetList()
        {
            if ( GetItems == null )
            {
                return new List<KeyValuePair<string, string>>();
            }
            var parentField = this.ParentGatewayField;
            var getItemsParams = new List<string>();
            while ( parentField != null )
            {
                getItemsParams.Add( parentField.CurrentValue );
            }
            return GetItems( getItemsParams );
        }
    }

    /// <summary>
    /// Interface to implement if your gateway requires redirection.
    /// </summary>
    public interface IRedirectionGateway
    {

        IEnumerable<RedirectionGatewayField> GatewayFields { get; }

        ///// <summary>
        ///// Gets the merchant field label.
        ///// </summary>
        ///// <value>
        ///// The merchant field label.
        ///// </value>
        //string MerchantFieldLabel { get; }

        ///// <summary>
        ///// Gets the fund field label.
        ///// </summary>
        ///// <value>
        ///// The fund field label.
        ///// </value>
        //string FundFieldLabel { get; }

        ///// <summary>
        ///// Gets the merchants.
        ///// </summary>
        ///// <returns></returns>
        //IEnumerable<KeyValuePair<string, string>> GetMerchants();

        ///// <summary>
        ///// Gets the merchant funds.
        ///// </summary>
        ///// <param name="merchantId">The merchant identifier.</param>
        ///// <returns></returns>
        //IEnumerable<KeyValuePair<string, string>> GetMerchantFunds(string merchantId);

    }
}
