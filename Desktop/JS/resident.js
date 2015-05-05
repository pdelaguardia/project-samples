eHealthClient.resident = function (params) {

    function showToast(message) {
        var device = DevExpress.devices.current();
        var toastSettings = {
            message: message,
            displayTime: 2000
        };
        DevExpress.ui.notify(toastSettings);
    };

    function Logout() {
        eHealthClient.app.UserName = "";
        eHealthClient.app.Password = "";
        var uri = eHealthClient.app.router.format({
            view: 'Login'
        });
        eHealthClient.app.navigate(uri);
    };

    function FormValidate(e) {
        if (viewModel.textboxName.value() != undefined && viewModel.textboxBirthplace.value() != undefined && viewModel.textboxBirthdate.value() != undefined && viewModel.selectboxSex.value() != undefined && viewModel.selectboxMS.value() != undefined && viewModel.textboxMAARet.value() != undefined && viewModel.textboxMAALTC.value() != undefined && viewModel.textboxMAASupp.value() != undefined)
            return true
        else
            return false;
    };

    function btnAceptance_OnClick(e) {
        if (FormValidate()) {
            var name = viewModel.textboxName.value();
            var birthplace = viewModel.textboxBirthplace.value();
            var birthdate = viewModel.textboxBirthdate.value().toDateString();
            var sex = viewModel.selectboxSex.value();
            var ms = viewModel.selectboxMS.value();
            var ltc = viewModel.textboxLTC.value();
            var mma = viewModel.textboxMMA.value();
            var ret = viewModel.textboxMAARet.value();
            var pens = viewModel.textboxMAALTC.value();
            var supp = viewModel.textboxMAASupp.value();
            eHealthClient.app.router.register(":view/:message", { view: "home", message: 'Done' });
            eHealthClient.db.get('InsertResident', { name: name, address: '', birthdate: birthdate, sex: sex, maritalstatus: ms, birthplace: birthplace, LTC: ltc, MMA: mma, retirement: ret, pension: pens, supp: supp });
            var uri = eHealthClient.app.router.format({
                view: 'Customers',
                message: 'Done'
            });
            eHealthClient.app.navigate(uri);
        }
        else {
            showToast("Missing Fields");
        }
    };

    function calcSupp(e) {
        viewModel.textboxMAASupp.value(viewModel.textboxMAARet.value() + viewModel.textboxMAALTC.value())
    };

    function chkLTCValidate(e) {
        if (viewModel.chkboxLTC.value())
            viewModel.textboxLTC.visible(true);
        else
            viewModel.textboxLTC.visible(false);
    };

    function chkMMAValidate(e) {
        if (viewModel.chkboxMMA.value())
            viewModel.textboxMMA.visible(true);
        else
            viewModel.textboxMMA.visible(false);
    };
    
    var viewModel = {
        Logout: Logout,

        textboxName: {
            value: ko.observable()
        },
        textboxBirthplace: {
            value: ko.observable()
        },
        textboxBirthdate: {
            value: ko.observable()
        },
        selectboxSex: {
            data: ["Male", "Female"],
            value: ko.observable()
        },
        selectboxMS: {
            data: ["Married", "Divorced", "Single", "Widowed"],
            value: ko.observable()
        },
        chkboxLTC: {
            value: ko.observable(),
            valueChangeAction: chkLTCValidate
        },
        chkboxMMA: {
            value: ko.observable(),
            valueChangeAction: chkMMAValidate
        },
        textboxLTC: {
            value: ko.observable(),
            visible: ko.observable(false)
        },
        textboxMMA: {
            value: ko.observable(),
            visible: ko.observable(false)
        },
        textboxMAARet: {
            value: ko.observable(),
           // valueChangeAction: calcSupp
        },
        textboxMAALTC: {
            value: ko.observable(),
          //  valueChangeAction: calcSupp
        },
        textboxMAASupp: {
            value: ko.observable(),
         //   readOnly: true
        },
        dxButtonAceptance: {
            text: "Aceptance",
            clickAction: btnAceptance_OnClick
        },
    };
    return viewModel;
};