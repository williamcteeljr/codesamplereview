(function (risk, $, undefined) {
    var _defaultProductLine = 'Default';
    /* Determines if a risk is new or renewal business based on the suffix.*/
    risk.getBusinessType = function (suffix, productLine) {
        var type = 'newBusiness';
        suffix = parseInt(suffix, 10);

        if (!isNaN(suffix)) {
            if (productLine == 'Workers Comp') {
                if (suffix > 0) {
                    type = 'renewal';
                }
            } else {
                type = (suffix > 1) ? "renewal" : type;
            }
        }

        return type;
    }

    var _alwaysRequiredInputs = ['BranchLbl', 'StatusLbl', 'ProductLineLbl', 'EffectiveDateLbl', 'ExpirationDateLbl', 'ProcessedDateLbl', 'BrokerLbl', 'UnderwriterLbl',
                                'NameLbl', 'CompanyLbl', 'CityStateLbl', 'RenewalUnderwriterLbl']

    risk.productLineStatusRequirements = [
        {
            productLine: 'Workers Comp',
            newBusiness: [
                {status: 'Submission', fields: _alwaysRequiredInputs },
                {status: 'Declined', previousStatus: 'Submission', fields: ['MarketLbl', 'CommissionLbl', 'AnnualPremLbl', 'WrittenPremLbl', 'AccountDescLbl', 'StatusReasonLbl'] },
                { status: 'Quote', previousStatus: 'Submission', fields: ['MarketLbl', 'CommissionLbl', 'AnnualPremLbl', 'WrittenPremLbl', 'AccountDescLbl'] },
                { status: 'Lost', previousStatus: 'Quote', fields: ['StatusReasonLbl'] },
                { status: 'Bound', previousStatus: 'Quote', fields: ['StreetAddressLbl'] },
                { status: 'Issued', previousStatus: 'Bound', fields: ['StreetAddressLbl'] },
                { status: 'Canceled', previousStatus: 'Issued', fields: ['StreetAddressLbl'] },
            ],
            renewal: [
                { status: 'Submission', fields: _alwaysRequiredInputs },
                { status: 'Declined', previousStatus: 'Submission', fields: ['CommissionLbl', 'AnnualPremLbl', 'WrittenPremLbl', 'AccountDescLbl', 'StatusReasonLbl'] },
                { status: 'Quote', previousStatus: 'Submission', fields: ['CommissionLbl', 'AnnualPremLbl', 'WrittenPremLbl', 'AccountDescLbl'] },
                { status: 'Lost', previousStatus: 'Quote', fields: ['StatusReasonLbl'] },
                { status: 'Bound', previousStatus: 'Quote', fields: ['StreetAddressLbl'] },
                { status: 'Issued', previousStatus: 'Bound', fields: ['StreetAddressLbl'] },
                { status: 'Canceled', previousStatus: 'Issued', fields: ['StreetAddressLbl'] },
            ]
        },
        {
            productLine: _defaultProductLine,
            newBusiness: [
                { status: 'Submission', fields: _alwaysRequiredInputs },
                { status: 'Declined', previousStatus: 'Submission', fields: ['CommissionLbl', 'AnnualPremLbl', 'WrittenPremLbl', 'StatusReasonLbl'] },
                { status: 'Quote', previousStatus: 'Submission', fields: ['CommissionLbl', 'AnnualPremLbl', 'WrittenPremLbl'] },
                { status: 'Lost', previousStatus: 'Quote', fields: ['StatusReasonLbl'] },
                { status: 'Bound', previousStatus: 'Quote', fields: ['StreetAddressLbl'] },
                { status: 'Issued', previousStatus: 'Bound', fields: ['StreetAddressLbl'] },
                { status: 'Canceled', previousStatus: 'Issued', fields: ['StreetAddressLbl'] },
            ],
            renewal: [
                { status: 'Submission', fields: $.merge( $.merge( [], _alwaysRequiredInputs ), ['ExpAnnualPremLbl', 'ExpWrittenPremLbl'] ) },
                { status: 'Declined', previousStatus: 'Submission', fields: [ 'CommissionLbl', 'AnnualPremLbl', 'WrittenPremLbl', 'StatusReasonLbl'] },
                { status: 'Quote', previousStatus: 'Submission', fields: [ 'CommissionLbl', 'AnnualPremLbl', 'WrittenPremLbl', 'PurposeOfUseLbl', 'AirportLbl'] },
                { status: 'Lost', previousStatus: 'Quote', fields: ['StatusReasonLbl'] },
                { status: 'Bound', previousStatus: 'Quote', fields: ['StreetAddressLbl'] },
                { status: 'Issued', previousStatus: 'Bound', fields: ['StreetAddressLbl'] },
                { status: 'Canceled', previousStatus: 'Issued', fields: ['StreetAddressLbl', 'StatusReasonLbl'] },
            ]
        }
    ];

    //Type == newBusiness : renwal
    risk.setRequiredFieldsForProductLineStatus = function (productLine, status, suffix) {
        if (status == orAero.statuses.AlreadyInvolved.Value)
            status = orAero.statuses.Submission.Value;

        orAero.productLines().then(function (res) {
            var productLineName = appUtilities.firstOrDefault(res, 'ProductLineId', productLine).Name;
            var type = risk.getBusinessType(suffix, productLineName);
            
            var plStatusReq = appUtilities.firstOrDefault(risk.productLineStatusRequirements, 'productLine', productLineName);

            if (plStatusReq === undefined) {
                plStatusReq = appUtilities.firstOrDefault(risk.productLineStatusRequirements, 'productLine', _defaultProductLine);
            }

            var typeStatusReq = plStatusReq[type];
            var statusReq = appUtilities.firstOrDefault(typeStatusReq, 'status', status);

            var fieldLabels = statusReq.fields;
            var prevStatus = statusReq.previousStatus;
        
            while (prevStatus !== undefined) {
                var priorStatusReq = appUtilities.firstOrDefault(typeStatusReq, 'status', prevStatus);
                $.merge(fieldLabels, priorStatusReq.fields);
                prevStatus = priorStatusReq.previousStatus;
            }

            $.unique(fieldLabels);

            risk.updateRequiredInputLabels(fieldLabels);
        })
    }

    risk.updateRequiredInputLabels = function (labels) {
        $('.control-label').css({ color: 'black' });
        $('.control-label').children('.req').hide();
        if (labels.length) {
            for (var i = 0; i < labels.length; i++) {
                $('#' + labels[i]).css({ color: '#d9534f' });
                $('#' + labels[i]).children('.req').show();
            }
        }
    }

    risk.update = function () {
        $("form#RiskEditForm .selectList option").attr("selected", "selected");

        ora.Ajax.FormPost(
        {
            url: "/PolicyTracker/Policy/RiskEditForm",
            container: "RiskEdit_dg",
            grid: "NamedInsuredSearchGrid",
            form: "RiskEditForm",
            data: $("#RiskEditForm").serializeArray(),
            PreventDefault: true,
            dialog: "RiskEdit_dg",
            successCallback: function () {
                $("#WLRefresh").click();
            }
        })
    }
}(window.orAero.risk = window.orAero.risk || {}, jQuery));

//(function () {

    ora.Risk = {
        AirportRiskTypes: ['CA', 'AG', 'AA', 'AVC', 'RAL'],

        setLabelsForStatus: function (productLine, status, suffix) {
            try {
                orAero.risk.setRequiredFieldsForProductLineStatus(productLine, status, suffix);
            } catch (error) {
                ora.Logging.Log(error, 'ora.Risk.setLabelsForStatus');
            }
        },

        NewRisk: function () {
            ora.Dialog.ActionForm({
                id: 'NewRiskEntry_dg',
                title: 'Entering a New Risk for a New Client',
                form: { id: "RiskEntryForm", container: "NewRiskEntry_dg", dialog: "NewRiskEntry_dg", submitURL: "policytracker/Policy/RiskEntryForm", grid: "NamedInsuredSearchGrid" },
                url: "/policytracker/Policy/RiskEntryForm",
                modal: true, autoFocus: "ImageRightId", 
                WritePermissions: true,
                CustomFormSubmit: true,
                Type: ora.Dialog.ActionFormSizes.Large,
            });
        },

        RiskEdit: function (riskId) {
            var risk = null;

            ora.Ajax.Ajax({
                url: "/policytracker/api/risk/GetRisk/" + riskId,
                dataType: "HTML",
                type: "GET",
                successCallback: function (data) {
                    var result = JSON.parse(data);
                    var canEdit = (result.Prefix != "AP" && result.Prefix != "AVC" && result.QuoteType != "AP" && result.QuoteType != "AVC");
                    ora.Dialog.ActionForm({
                        id: 'RiskEdit_dg',
                        title: 'Editing Risk ' + result.RiskId + " For " + result.Name,
                        form: { id: "RiskEditForm", container: "RiskEdit_dg", submitURL: "/policytracker/Policy/RiskEditForm", grid: "RiskFindGrid" },
                        url: "/policytracker/Policy/RiskEditForm/" + riskId,
                        modal: true, autoFocus: "ImageRightId",
                        WritePermissions: true,
                        CustomFormSubmit: true,
                        Type: ora.Dialog.ActionFormSizes.Large,
                    });
                }
            });
        },

        GetAirportState: function () {
            var state = '';
            try {
                var apCombo = ASPxClientControl.GetControlCollection().GetByName('AirportId');
                var apIndex = apCombo.GetSelectedIndex();
                var state = (apIndex == -1) ? "" : apCombo.GetSelectedItem().GetColumnText('State');
            } catch (error) {
                ora.Logging.Log(error, 'ora.Risk.GetAirportState');
            }
            
            return state;
        },

        BrokerIndexChange: function (s, e) {
            try {
                var index = s.GetSelectedIndex();
                var riskId = $('#Id').val();

                if (index == -1) {
                    $('#BrokerName').val('');
                    $('#BrokerState').val('');
                    $('#AgentName').val('');
                } else {
                    $('#BrokerName').val(s.GetSelectedItem().GetColumnText('AgencyName'));
                    $('#BrokerState').val(s.GetSelectedItem().GetColumnText('State'));
                }

                var agencyId = (index == -1) ? "" : s.GetSelectedItem().GetColumnText('AgencyID');

                if (agencyId != "") {
                    ora.Ajax.Ajax({
                        url: "/policytracker/api/brokers/getbroker?brokercode=" + agencyId,
                        dataType: "Json",
                        type: "GET",
                        successCallback: function (data) {
                            console.log(data);
                            if (data.IsMidTermOnly == true) {
                                ora.Dialog.Simple({
                                    message: "ORA no longer quotes new business for broker " + data.AgencyID + " - " + data.AgencyName
                                });
                                $("#NewRiskEntry_dg").dialog("destroy").remove();
                            }
                        }
                    });
                }

                var productLine = $("#ProductLine").val();
                var product = $("#Prefix").val();
                var suffix = $("#PolicySuffix").val();

                ora.Ajax.Ajax({
                    url: "/policytracker/Policy/GetBrokerCommission?agencyId=" + agencyId + "&productLine=" + productLine + "&suffix=" + suffix + "&prefix=" + product,
                    dataType: "HTML",
                    type: "GET",
                    successCallback: function (data) {
                        $("#Commission").val(data);
                        if (riskId > 0) {
                            $("#globalToast").dxToast('instance').option('message', 'Commission set to ' + data + '%');
                            $("#globalToast").dxToast("instance").show();
                        }
                    }
                });

                //ASPxClientControl.GetControlCollection().GetByName('AgentId').PerformCallback();

                ora.Ajax.Ajax({
                    url: "/policytracker/api/brokers/GetBrokerAgents?brokerCode=" + agencyId,
                    dataType: "Json",
                    type: "GET",
                    successCallback: function (data) {
                        $("#AgentId").empty();
                        $.each(data, function (value, agent) {
                            $("#AgentId").append($("<option></option>")
                               .attr("value", agent.IndID).text(agent.Name));
                        });
                        $("#AgentId").val(0);
                    }
                });

                ora.Risk.UpdateBrokerLicenseNotification();

                //Underwriter Auto Assign
                if (productLine != "" && agencyId != "" && riskId == 0) {
                    ora.Risk.UnderwriterAutoassignment()
                }
            } catch (error) {
                ora.Logging.Log(error, 'ora.Risk.BrokerIndexChange');
            }
        },

        UnderwriterAutoassignment: function () {
            try {
                var brokerCombo = ASPxClientControl.GetControlCollection().GetByName('AgencyID');
                var index = brokerCombo.GetSelectedIndex();
                var broker = (index == -1) ? "" : brokerCombo.GetSelectedItem().GetColumnText('AgencyID');
                var productLine = $("#ProductLine").val();

                if (productLine != "" && broker != "") {
                    ora.Ajax.Ajax({
                        url: "/policytracker/api/risk/GetBrokerAssignment",
                        data: { broker: broker, productLine: productLine },
                        successCallback: function (data) {
                            if (data != null) {
                                $("#UnderwriterId").val(data.UserId);
                                $("#globalToast").dxToast('instance').option('message', 'Underwriter Updated/Changed Based on Broker & Product Line');
                                $("#globalToast").dxToast("instance").show();
                            }
                        }
                    });
                }
            } catch (error) {
                ora.Logging.Log(error, 'ora.Risk.UnderwriterAutoassignment');
            }
        },

        SendBrokerLicenseNotification: function (form) {
            try {
                var brokerWidget = ASPxClientControl.GetControlCollection().GetByName('AgencyID');
                var agencyId = (brokerWidget.GetSelectedItem() != null) ? brokerWidget.GetSelectedItem().GetColumnText('AgencyID') : null;
                var airportWidget = ASPxClientControl.GetControlCollection().GetByName('AirportId');
                var state = (airportWidget.GetSelectedItem() != null) ? airportWidget.GetSelectedItem().GetColumnText('State') : null;

                var policyPrefix = $('#Prefix').val();
                if (ora.Risk.AirportRiskTypes.indexOf(policyPrefix) == -1) {
                    state = $('#State').val();
                }

                var riskId = $('form#' + form + ' > #Id').val();

                if (state && agencyId) {
                    ora.Ajax.Ajax({
                        url: "/policytracker/api/Compliance/LicenseNotification?State=" + state + "&agencyId=" + agencyId + "&riskId=" + riskId + "&airportId=" + airportWidget,
                        dataType: "Json",
                        type: "GET",
                        successCallback: function () {
                            $("#globalToast").dxToast('instance').option({ message: 'Compliance has been notified of the licensing issue', type: 'success' }),
                            $("#globalToast").dxToast("instance").show();
                        }
                    });
                    //ora.Ajax.Ajax({
                    //    type: 'GET',
                    //    url: '/policytracker/api/Compliance/LicenseNotification',
                    //    data: { State: state, RiskId: riskId, AgencyId: agencyId, Airport: airportWidget },
                    //    successCallback: function () {
                    //        $("#globalToast").dxToast('instance').option({ message: 'Compliance has been notified of the licensing issue', type: 'success' }),
                    //        $("#globalToast").dxToast("instance").show();
                    //    }
                    //});
                } else {
                    $("#globalToast").dxToast('instance').option({ message: 'Must select a state and agency for risk before E-Mailing compliance', type: 'warning' }),
                    $("#globalToast").dxToast("instance").show();
                }
            } catch (error) {
                ora.Logging.Log(error, 'ora.Risk.SendBrokerLicenseNotification');
            }
        },

        AirportIndexChange: function (s, e) {
            ora.Risk.UpdateBrokerLicenseNotification();
        },

        StatusChange: function (formId) {
            try {
                var status = $("#Status").val();

                ora.Ajax.Ajax({
                    url: "/policytracker/api/risk/GetStatusReasons",
                    data: { status: status },
                    dataType: "Json",
                    type: "GET",
                    successCallback: function (data) {
                        $("form#" + formId + " #StatusReason").empty();
                        $("#StatusReasonExamplesPopup").empty();

                        $.each(data, function (value, reason) {
                            $("#StatusReason").append($("<option></option>")
                               .attr("value", reason.Id).text(reason.Reason));
                            $("#StatusReasonExamplesPopup").append($("<div></div>")
                                .html("<p>" + reason.Reason + " - " + reason.ReasonDesc + "</p>"))
                        });

                        $("#StatusReason").val(0);
                    }
                });

                if (status == "Declined" || status == 'Lost' || status == 'Canceled') {
                    $('form#' + formId + ' #Reasondd').show();
                    //$('form#' + formId).find('.RFD').each(function () {
                    //    $(this).show();
                    //    var inputElement = $(this).attr('data-dec-field');
                    //})
                } else if (status == "Lost") {
                    $('form#' + formId + ' #Reasondd').show();
                } else {
                    $('form#' + formId + ' #Reasondd').hide();
                    //$('form#' + formId).find('.RFD').each(function () {
                    //    $('#Reasondd').hide();
                    //    $(this).hide();
                    //    var inputElement = $(this).attr('data-dec-field');
                    //})
                }
            } catch (error) {
                ora.Logging.Log(error, 'ora.Risk.StatusChange');
            }
	    },

	    ClientTypeChange: function () {
		    $("#IndividualInput").toggle();
		    $("#CorpInput").toggle();
	    },

	    WorkersCompPolicyCodes: ["CAD", "CAN", "CAV", "CAW", "CDB", "CAL", "CAR"],

	    ProductLineChange: function () {
	        try {
	            var dxBrokerControl = ASPxClientControl.GetControlCollection().GetByName('AgencyID');
	            var index = dxBrokerControl.GetSelectedIndex();
	            var riskId = $('#Id').val();

	            var agencyId = (index == -1) ? "" : dxBrokerControl.GetSelectedItem().GetColumnText('AgencyID');
	            var productLine = $("#ProductLine").val();
	            var product = $("#Prefix").val();
	            var suffix = $("#PolicySuffix").val();

	            ora.Ajax.Ajax({
	                url: "/policytracker/Policy/GetBrokerCommission?agencyId=" + agencyId + "&productLine=" + productLine + "&suffix=" + suffix + "&prefix=" + product,
	                dataType: "HTML",
	                type: "GET",
	                successCallback: function (data) {
	                    $("#Commission").val(data);
	                    if (riskId > 0) {
	                        $("#globalToast").dxToast('instance').option('message', 'Commission set to ' + data + '%');
	                        $("#globalToast").dxToast("instance").show();
	                    }
	                }
	            });

	            if (productLine == "Workers Comp") {
	                $("#WCSection").show();
	            } else {
	                $("#WCSection").hide();
	            }

	            ora.Risk.UpdateBrokerLicenseNotification();
	        } catch (error) {
	            ora.Logging.Log(error, 'ora.Risk.ProductLineChange');
	        }
	    },

        //Update the Status of the Broker License Notification
	    UpdateBrokerLicenseNotification: function () {
	        try {
	            var brokerWidget = ASPxClientControl.GetControlCollection().GetByName('AgencyID');
	            var agencyId = (brokerWidget.GetSelectedItem() != null) ? brokerWidget.GetSelectedItem().GetColumnText('AgencyID') : null;
	            var airportWidget = ASPxClientControl.GetControlCollection().GetByName('AirportId');
	            var state = (airportWidget.GetSelectedItem() != null) ? airportWidget.GetSelectedItem().GetColumnText('State') : null;
	            var riskId = $('form#' + form + ' > #Id').val();
	            var policyPrefix = $('#Prefix').val();
	            if (ora.Risk.AirportRiskTypes.indexOf(policyPrefix) == -1) {
	                state = $('#State').val();
	            }

	            if ((agencyId && state) && state != "N/A") {
	                ora.Ajax.Ajax({
	                    url: "/policytracker/Policy/IsBrokerLicensedForState?agencyId=" + agencyId + "&state=" + state + "&RiskId=" + riskId,
	                    dataType: "HTML",
	                    type: "GET",
	                    successCallback: function (data) {
	                        if (data == "True") {
	                            $('#BrokerLicenseIssue').hide();
	                        } else {
	                            $('#BrokerLicenseIssue').show();
	                        }
	                    }
	                });
	            } else {
	                $('#BrokerLicenseIssue').hide();
	            }
	        } catch (error) {
	            ora.Logging.Log(error, 'ora.Risk.UpdateBrokerLicenseNotification');
	        }
	    },

	    CheckOFACList: function (name) {
	        try {
	            if (!appUtilities.isNullOrWhitespace(name)) {
	                ora.Ajax.Ajax({
	                    type: 'GET',
	                    url: '/policytracker/api/risk/CheckOFACSDN',
	                    data: { name: name },
	                    successCallback: function (data) {
	                        if (data.TotalHits > 0) {
	                            $('#OFACWarning').show();
	                            $('#HasFailedOFAC').prop('checked', true);
	                        } else {
	                            $('#OFACWarning').hide();
	                        }
	                    }
	                });
	            }
	        } catch (error) {
	            ora.Logging.Log(error, 'ora.Risk.CheckOFACList');
	        }
	    }
    }
//})();