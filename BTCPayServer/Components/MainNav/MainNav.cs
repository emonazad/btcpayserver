using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BTCPayServer.Controllers;
using BTCPayServer.Data;
using BTCPayServer.Models.StoreViewModels;
using BTCPayServer.Payments;
using BTCPayServer.Payments.Lightning;
using BTCPayServer.Services.Apps;
using BTCPayServer.Services.Invoices;
using BTCPayServer.Services.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NBitcoin;
using NBitcoin.Secp256k1;

namespace BTCPayServer.Components.MainNav
{
    public class MainNav : ViewComponent
    {
        private const string RootName = "Global";
        private readonly AppService _appService;
        private readonly StoreRepository _storeRepo;
        private readonly StoresController _storesController;
        private readonly BTCPayNetworkProvider _networkProvider;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PaymentMethodHandlerDictionary _paymentMethodHandlerDictionary;

        public MainNav(
            AppService appService,
            StoreRepository storeRepo,
            StoresController storesController,
            BTCPayNetworkProvider networkProvider,
            UserManager<ApplicationUser> userManager,
            PaymentMethodHandlerDictionary paymentMethodHandlerDictionary)
        {
            _storeRepo = storeRepo;
            _appService = appService;
            _userManager = userManager;
            _networkProvider = networkProvider;
            _storesController = storesController;
            _paymentMethodHandlerDictionary = paymentMethodHandlerDictionary;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var store = ViewContext.HttpContext.GetStoreData();
            var vm = new MainNavViewModel { Store = store };
#if ALTCOINS
            vm.AltcoinsBuild = true;
#endif
            if (store != null)
            {
                var storeBlob = store.GetStoreBlob();

                // Wallets
                _storesController.AddPaymentMethods(store, storeBlob,
                    out var derivationSchemes, out var lightningNodes);
                vm.DerivationSchemes = derivationSchemes;
                vm.LightningNodes = lightningNodes;

                // Apps
                var apps = await _appService.GetAllApps(UserId, false, store.Id);
                vm.Apps = apps.Select(a => new StoreApp
                {
                    Id = a.Id,
                    AppName = a.AppName,
                    AppType = a.AppType,
                    IsOwner = a.IsOwner
                }).ToList();
            }

            return View(vm);
        }

        private string UserId => _userManager.GetUserId(HttpContext.User);
    }
}
