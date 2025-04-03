var loader;
//var todaySpent = 0.00;
//var availableAmount = 0.00;
//var excessAmount = 0.00;
//var daybudgetAmount = 120.00;
var TodaySpent = parseFloat($("#todaySpentAmount").text().replace('$', '').trim()) || 0;
var TodayAvailable = 120.00;
var TodayExcess = parseFloat($('#excessAmount').text()) || 0;
var TodayTotal = 120.00;

$(document).ready(function () {
    
    loader = {
        show: function () {
            $(".loader").show(); // Shows the spinner
        },
        hide: function () {
            $(".loader").hide(); // Hides the spinner
        }
    };
    toastr.options = {
        "positionClass": "toast-bottom-right",
        "closeButton": true,
        "debug": false,
        "newestOnTop": true,
        "progressBar": true,
        "preventDuplicates": true,
        "onclick": null,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };
    var today = new Date();
    var dd = String(today.getDate()).padStart(2, '0');
    var mm = String(today.getMonth() + 1).padStart(2, '0'); 
    var yyyy = today.getFullYear();
    today = yyyy + '-' + mm + '-' + dd;
    $('#dateFilter').val(today);
    
    $('#category').select2();
    $(".select2").css("width", "100%");
    $(".select2").css("height", "100%");
    Maintable();
    TableHeight();
    GetCategory();
    $('.select2').select2();
    $('#category').select2({
        width: '100%'  
    });
    $("#saveExpenseBtn").on('click', function () {
        SaveExpense();
    });

    

});


$(document).on('change', '#dateFilter', function () {
    if ($(this).val()) {
        Maintable($(this).val());
        if ($(window).width() < 500) {
            $('.navbar-toggler').click();
        }
        setTimeout(function () {
            TrackAmount();
        }, 500);
    }
});

function Maintable(value) {
    var dateValue;
    let filterText = $("#filterText");
    var types = $("#filterText").text();
    if (value == "" || value == null) {
        dateValue = $('#dateFilter').val();
    } else {
        dateValue = value;
    }

    loader.show();

    // Destroy the existing DataTable before making the AJAX request
    if ($.fn.DataTable.isDataTable('#expensetable')) {
        $('#expensetable').DataTable().clear().destroy();
    }

    $.ajax({
        type: "POST",
        url: "/Expense/GetTableData",
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({ dateValue: dateValue, types: types }),
        dataType: "json",
        success: function (mdata) {
            var mobj = mdata;
            var tableBody = $('#tblBody');
            tableBody.empty(); 
            loader.show();
            if (mobj.Data && mobj.Data.distinctTable) {
                var distinctTable = JSON.parse(mobj.Data.distinctTable);
                $.each(distinctTable.query, function (index, item) {
                    var tableRow = '<tr>';

                    // Dropdown HTML construction for the first column
                    var mstring = "<div class='dropdown'>" +
                        "<button id='btnGroupDrop" + index + "' type='button' class='btn btn-warning btn-sm dropdown-toggle' data-bs-toggle='dropdown' aria-haspopup='true' aria-expanded='false'>" +  // Updated for Bootstrap 5
                        "<i class='fa fa-cogs fa-xs'></i>" +
                        "</button>" +
                        "<div class='dropdown-menu' aria-labelledby='btnGroupDrop" + index + "'>" +
                        "<a class='dropdown-item edit-item' href='#' data-id='" + item.id + "' onclick='EditExpense(" + item.id + ")'><i class='fa fa-edit'></i> Edit</a>" +
                        "<a class='dropdown-item delete-item' href='#' data-id='" + item.id + "' onclick='DeleteExpense(" + item.id + ")'><i class='fa fa-trash'></i> Delete</a>" +
                        "</div></div>";

                    // Add dropdown to the first column
                    tableRow += '<td>' + mstring + '</td>';

                    // Add other columns
                    tableRow += '<td>' + item.category + '</td>';
                    tableRow += '<td>' + item.DESCRIPTION + '</td>';

                    // Format Date
                    var date = new Date(item.Date);
                    var formattedDate = date.toLocaleDateString();
                    tableRow += '<td>' + formattedDate + '</td>';

                    // Add the 'Amount' column
                    tableRow += '<td>' + item.Amount + '</td>';

                    // Add 'Addedby' column
                    tableRow += '<td>' + item.Addedby + '</td>';

                    // Format AddedOn
                    var addedOn = new Date(item.AddedOn);
                    var formattedAddedOn = addedOn.toLocaleDateString();
                    tableRow += '<td>' + formattedAddedOn + '</td>';
                    tableRow += '</tr>';

                    // Append the row to the table body
                    $('#tblBody').append(tableRow);

                   
                    //var dropdowns = document.querySelectorAll('.dropdown-toggle');
                    //dropdowns.forEach(function (dropdown) {
                    //    new bootstrap.Dropdown(dropdown); 
                    //});
                });

                // Update the todaySpentAmount display
               // $('#todaySpentAmount').text('$' + mobj.Data[0].totalAmount.toFixed(2));
               // $('#todaySpentAmount').text('$' + distinctTable.query[0].totalAmount.toFixed(2));
                //$('#todaySpentAmount').text('$' + totalsAmount.query[0].totalAmount.toFixed(2));

                // Re-initialize DataTable after new data is added
                $('#expensetable').DataTable({
                    "dom": "<'row'<'col-md-4'l><'col-md-4 pagination-md flex-md-wrap paging-center'p><'col-md-4 mt-0'f>>",
                    "orderCellsTop": true,
                    "fixedHeader": {
                        header: true,
                        footer: true
                    },
                    responsive: true,
                    "lengthMenu": [[10, 25, 50, 100], [10, 25, 50, 100]],
                    pageLength: 10,
                    stateSave: true,
                    "bProcessing": false,
                    "bServerSide": false,
                    "bDestroy": true, // Allow re-initialization
                    "bFilter": true,
                    "sPaginationType": "full_numbers",
                    "bInfo": true,
                    "bLengthChange": true,
                    "bScrollCollapse": true,
                    "columnDefs": [
                        { "orderable": false, "targets": 0 } // Disable sorting on the first column (dropdown)
                    ],
                    "aaSorting": [[4, 'desc']],
                    language: {
                        oPaginate: {
                            sNext: '<i class="fa fa-forward"></i>',
                            sPrevious: '<i class="fa fa-backward"></i>',
                            sFirst: '<i class="fa fa-step-backward"></i>',
                            sLast: '<i class="fa fa-step-forward"></i>'
                        }
                    }
                });
                $('#expensetable').on('draw.dt', function () {
                    $('#expensetable thead tr th:first-child').removeClass('sorting_disabled');
                    $('#expensetable thead tr td:first-child').removeClass('sorting_disabled');
                });

                
            } else {
                var mgs = "No Data Available.";
                var tableRow = '<tr>';
                tableRow += '<td colspan="7" style="text-align:center;">' + mgs + '</td>';
                tableRow += '</tr>';
                $('#tblBody').html(tableRow);
            }
        },
        error: function () {
            setTimeout(function () { loader.hide(); }, 300);
            alert('Error fetching data!');
        },
        complete: function () {
            setTimeout(function () { loader.hide(); }, 300);
            TrackAmount();
            //$('.dropdown').select2({
            //    dropdownParent: $('#expensetable')
            //});
        }
    });
}


function validatesInput(input) {
    input.value = input.value.trimStart();  // Trim leading spaces
    if (input.value.replace(/\s+/g, '') === '') { // Remove spaces and check if empty
        input.value = '';  // Set to empty if it's just whitespace
    }
}
function TableHeight() {
    var mWidth = $(window).width();
    var mHeight = $(window).height();
    var widthCategory;
    if (mWidth < 600) widthCategory = "small";
    else if (mWidth >= 1024 && mWidth <= 1326) widthCategory = "medium";
    else if (mWidth > 767 && mWidth < 1024) widthCategory = "tablet";
    else if (mWidth >= 1366) widthCategory = "large";
    else widthCategory = "default";
    switch (widthCategory) {
        case "small":
            $("#divList").css({ "height": (mHeight - 180) + "px", "overflow": "auto" });
            $("#divListcategory").css({ "height": (mHeight - 180) + "px", "overflow": "auto" });
            break;

        case "medium":
            $("#divList").css({ "height": (mHeight - 150) + "px", "overflow": "auto" });
            $("#divListcategory").css({ "height": (mHeight - 150) + "px", "overflow": "auto" });
            break;

        case "tablet":
            $("#divList").css({ "height": (mHeight - 160) + "px", "overflow": "auto" });
            $("#divListcategory").css({ "height": (mHeight - 160) + "px", "overflow": "auto" });
            break;

        case "large":
            if (mHeight >= 500 && mHeight < 641) {
                $("#divList").css({ "height": (mHeight - 230) + "px", "overflow": "auto" });
                $("#divListcategory").css({ "height": (mHeight - 230) + "px", "overflow": "auto" });
            } else if (mHeight >= 641) {
                $("#divList").css({ "height": (mHeight - 220) + "px", "overflow": "auto" });
                $("#divListcategory").css({ "height": (mHeight - 220) + "px", "overflow": "auto" });
            }
            break;

        default:
            $("#divtable").css({ "height": (mHeight - 80) + "px", "overflow": "auto" });
            $("#divListcategory").css({ "height": (mHeight - 80) + "px", "overflow": "auto" });
            break;
    }
}

function AddNewExpense(rowId) {
    if ($(window).width() < 500) {
        $('.navbar-toggler').click();
    }
    var params = rowId !== undefined ? rowId : "";

    if (params == "") {
        $(".modal-title").text("Add New Expense");
    }
    else {
        $(".modal-title").text("Edit Expense (" + rowId + ")");
    }
    $("#expensemodal").modal('show');
    $('#category').select2({
        dropdownParent: $('#expensemodal')
    });
   // $('#category').next('.select2-container').find('.select2-selection').css('display', 'none');
}

function GetCategory() {
    $.ajax({
        url: '/Expense/GetCategory',
        type: 'POST',
        dataType: "json",
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            if (response.ErrNum == 0) {  
                var categories = response.Data.distinctCategory.query;
                $('#category').empty();
                $('#category').append('<option value="">Select Category</option>');
                $.each(categories, function (i, u) {
                    $("#category").append(
                        '<option value="' + u.cad_id + '">' + u.cad_name + '</option>'
                    );
                });
            } else {
                toastr.error("Error fetching categories:", response.Message);
            }
        },
        error: function (err) {
            toastr.error("Error fetching categories:", err);
        }
    });

}

function SaveExpense() {
    if ($('#category').val() == "" || $('#category').val() == null) {
        $('#category').focus();
        //notyf.error('Please select category.');
        toastr.warning('Please select category.');
        return;
    }
    if ($('#description').val() == "" || $('#description').val() == null) {
        $('#description').focus();
        //notyf.error('Please enter description.');
        toastr.warning('Please enter description.');
        return;
    }
    if ($('#date').val() == "" || $('#date').val() == null) {
        $('#date').focus();
        //notyf.error('Please select date.');
        toastr.warning('Please select date.');
        return;
    }
    if ($('#amount').val() == "" || $('#amount').val() == null) {
        $('#amount').focus();
        //notyf.error('Please enter amount.');
        toastr.warning('Please enter amount.');
        return;
    }
    var date = new Date($('#date').val());
    var formattedDate = date.toISOString().slice(0, 19).replace('T', ' '); 
    var formData = {
        category: $('#category').val(),
        description: $('#description').val(),
        amount: $('#amount').val(),
        date: formattedDate
    };
    loader.show();
    $.ajax({
        type: "POST",
        url: '/Expense/SaveExpense',  
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(formData),  
        dataType: "json",
        success: function (response) {
            if (response.ErrNum == 0) {
                document.activeElement.blur();
                toastr.success(response.ErrMsg);
                $("#expensemodal").modal('hide');
                ClearValue();
                Maintable($('#date').val());
                setTimeout(function () {
                    TrackAmount();
                }, 500);
                
            } else {
                toastr.error("Error: " + response.ErrMsg);
            }
            loader.hide();
        },
        error: function () {
            toastr.error("Error: " + response.ErrMsg);
            loader.hide();
        }
    });
}



$(document).on("click", ".closeExpenseBtn", function () {
    var modalID = $(this).data("modal");
    document.activeElement.blur();
    if (modalID == "expensecategory") {
        $('#showadd').hide();
        $('#categoryview').hide();

    }
    $("#" + modalID).modal('hide');
    if ($(window).width() < 500) {
        $('.navbar-toggler').click();
    }
});

function ClearValue() {
    $('#category').val('');
    $('#description').val('');
    $('#date').val('');
    $('#amount').val('');
}

function EditExpense(rowId) {
    var rowValue = rowId;
    $.ajax({
        type: "POST",
        url: '/Expense/EditExpense',
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({ rowValue:rowValue }),
        dataType: "json",
        success: function (response) {
            ClearValue();
            if (response.ErrNum == 0) {
                var distinctTable = JSON.parse(response.Data.distinctTableValue);
                $("#expensemodal").modal('show');
                $('#category').select2({
                    dropdownParent: $('#expensemodal')
                });
                $(".modal-title").text("Edit Expense (" + distinctTable.query[0].id + ")");
                var categoryId = distinctTable.query[0].categ_id;
                $('#category').val(categoryId).trigger('change');
                $('#description').val(distinctTable.query[0].DESCRIPTION);
                $('#date').val(distinctTable.query[0].Date.split('T')[0]);
                $('#amount').val(distinctTable.query[0].Amount);

            } else {
                toastr.error("Error: " + response.ErrMsg);
            }
            loader.hide();
        },
        error: function () {
            toastr.error("Error: " + response.ErrMsg);
            loader.hide();
        }
    });
}

//
function DeleteExpense(rowId) {
    var rowValue = rowId;
    Swal.fire({
        title: 'Are you sure?',
        text: "Do you want to delete this expense?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete it!',
        cancelButtonText: 'No, cancel!',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: "POST",
                url: '/Expense/DeleteExpense',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ rowValue: rowValue }),
                dataType: "json",
                success: function (response) {
                    if (response.ErrNum == 0) {
                        toastr.success(response.ErrMsg);
                        Maintable($('#date').val());
                        setTimeout(function () {
                            TrackAmount();
                        }, 500);
                    } else {
                        toastr.error("Error: " + response.ErrMsg);
                    }
                    loader.hide();
                },
                error: function () {
                    toastr.error("Error: " + response.ErrMsg);
                    loader.hide();
                }
            });
        } 
    });
}

function TrackAmount() {
    //$('#todaySpentAmount').text('$' + '0.00');
    //$('#availableAmount').text('$' + '120.00');
    //$('#excessAmount').text('$' + '0.00');
    //$('#dayBudgetAmount').text('$' + '120.00');
    var rowValue = $('#dateFilter').val();

    $.ajax({
        type: "POST",
        url: '/Expense/TracksAmount',
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify({ dateValue: rowValue }),
        dataType: "json",
        success: function (response) {
            if (response.ErrNum == 0) {
                var totalsAmount = JSON.parse(response.Data.totalAmount);
                if (totalsAmount.query[0].totalAmount != null) {
                    $('#todaySpentAmount').text('$' + totalsAmount.query[0].totalAmount.toFixed(2));
                    TodaySpent = totalsAmount.query[0].totalAmount;
                    TodayAvailable = TodayTotal - TodaySpent;
                    if (TodayAvailable < 0) {
                        $("#availableAmount").val("Exceed Today limit");
                        $('#availableAmount').text('$' + 'Exceed Today limit');
                    } else {
                        $('#availableAmount').text('$' + TodayAvailable.toFixed(2));
                    }

                    if (TodaySpent > 120) {
                        TodayExcess = (TodaySpent - 120);
                        $('#excessAmount').text('$' + TodayExcess.toFixed(2));
                    } else {
                        $('#excessAmount').text('$' + '0.00');
                    }
                }
                else {
                    TodaySpent = 0.00;
                    TodayAvailable = 120.00;
                    TodayExcess = 0.00;
                    TodayTotal = 120.00;
                    $('#todaySpentAmount').text(TodaySpent);
                    $('#availableAmount').text(TodayAvailable);
                    $('#excessAmount').text(TodayExcess);
                    $('#dayBudgetAmount').text(TodayTotal);
                }
                
            }
            
        },
        error: function () {
            loader.hide();
        }
    });
}

function validateField(selector, message) {
    if ($(selector).val() == "" || $(selector).val() == null) {
        $(selector).focus();
        toastr.warning(message);
        return false;
    }
    return true;
}

//#region Login


//#endregion


function ViewCategory() {

    $(".modal-title-category").text("View Category");
    $('#categoryview').show();
    $("#expensecategory").modal('show');
    CategoryTable();
    TableHeight(); 
   
}

function CategoryTable() {
    loader.show();
    $.ajax({
        type: "POST",
        url: '/Expense/GetViewCategory',
        contentType: "application/json; charset=utf-8",
        data: null,
        dataType: "json",
        success: function (mdata) {
            var mobj = mdata;
            var tableBody = $('#tblBodycategory');
            tableBody.empty();
            if (mobj.Data && mobj.Data.distinctTablecategory) {
                var distinctTable = JSON.parse(mobj.Data.distinctTablecategory);
                $.each(distinctTable.query, function (index, item) {
                    var tableRow = '<tr>';

                    // Dropdown HTML construction for the first column
                    var mstring = "<div class='dropdown'>" +
                        "<button id='btnGroupDrop" + index + "' type='button' class='btn btn-warning btn-sm dropdown-toggle' data-bs-toggle='dropdown' aria-haspopup='true' aria-expanded='false'>" +  // Updated for Bootstrap 5
                        "<i class='fa fa-cogs fa-xs'></i>" +
                        "</button>" +
                        "<div class='dropdown-menu' aria-labelledby='btnGroupDrop" + index + "'>" +
                        "<a class='dropdown-item edit-item' href='#' data-id='" + item.cad_id + "' onclick='EditCategory(" + item.cad_id + ", \"" + item.cad_name + "\", \"" + item.description + "\")'><i class='fa fa-edit'></i> Edit</a>" +
                        "<a class='dropdown-item delete-item' href='#' data-id='" + item.cad_id + "' onclick='DeleteCategory(" + item.cad_id + ")'><i class='fa fa-trash'></i> Delete</a>" +
                        "</div></div>";

                    // Add dropdown to the first column
                    tableRow += '<td>' + mstring + '</td>';

                    // Add other columns
                    tableRow += '<td>' + item.cad_name + '</td>';
                    tableRow += '<td>' + item.description + '</td>';

                    // Add 'Addedby' column
                    tableRow += '<td>' + item.addedby + '</td>';

                    // Format AddedOn
                    var addedOn = new Date(item.addedon);
                    var formattedAddedOn = addedOn.toLocaleDateString();
                    tableRow += '<td>' + formattedAddedOn + '</td>';
                    tableRow += '</tr>';

                    // Append the row to the table body
                    $('#tblBodycategory').append(tableRow);
                });


            } else {
                var mgs = "No Data Available.";
                var tableRow = '<tr>';
                tableRow += '<td colspan="7" style="text-align:center;">' + mgs + '</td>';
                tableRow += '</tr>';
                $('#tblBodycategory').html(tableRow);
            }
        },
        error: function () {
            toastr.error("Error: " + response.ErrMsg);
            loader.hide();
        },
        complete: function () {
            loader.hide();
        }
    });
}

$(document).on("click", "#newcategory", function () {
    $('#categoryview').hide();
    $('#showadd').show();
});

$(document).on("click", "#saveCategorybutton", function () {
    if (!validateField('#newcategoryname', 'Please enter username.')) return;
    if (!validateField('#newdescription', 'Please enter firstname.')) return;
    var formData = {
        categoryName: $('#newcategoryname').val(),
        categoryDescription: $('#newdescription').val(),
        categoryrowId: $('#editrowcategory').val()
    };
    loader.show();
    $.ajax({
        type: "POST",
        url: '/Expense/SaveNewCategory',
        contentType: "application/json; charset=utf-8",
        data: JSON.stringify(formData),
        dataType: "json",
        success: function (response) {
            if (response.ErrNum == 0) {
                loader.hide();
                toastr.success(response.ErrMsg);
                CategoryTable();
                TableHeight();
                GetCategory();
                $('#categoryview').show();
                $('#showadd').hide();
            }
            else {
                loader.hide();
                toastr.error("Error: " + response.ErrMsg);
            }
           
        },
        error: function () {
            toastr.error("Error: " + response.ErrMsg);
            loader.hide();
        }
    });
});

function EditCategory(rowId,rowCategory,rowDescription) {
    var rowValue = rowId;
    var rowsCategory = rowCategory;
    var rowsDescription = rowDescription;
    $('#categoryview').hide();
    $('#showadd').show();
    $('#newcategoryname').val(rowsCategory);
    $('#newdescription').val(rowsDescription);
    $('#editrowcategory').val(rowValue);
}

//
function DeleteCategory(rowId) {
    var rowValue = rowId;
    Swal.fire({
        title: 'Are you sure?',
        text: "Do you want to delete this expense?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Yes, delete it!',
        cancelButtonText: 'No, cancel!',
        reverseButtons: true
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: "POST",
                url: '/Expense/DeleteCategory',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ rowValue: rowValue }),
                dataType: "json",
                success: function (response) {
                    if (response.ErrNum == 0) {
                        toastr.success(response.ErrMsg);
                        Maintable($('#date').val());
                        setTimeout(function () {
                            TrackAmount();
                        }, 500);
                    } else {
                        toastr.error("Error: " + response.ErrMsg);
                    }
                    loader.hide();
                },
                error: function () {
                    toastr.error("Error: " + response.ErrMsg);
                    loader.hide();
                }
            });
        }
    });
}



//endregion

//#region Export csv and Pdf 
function exportFile(fileType) {
    loader.show();
    let dateValue = document.getElementById("dateFilter").value || "All";
    let fileName = `ExpenseData_${dateValue}.${fileType.toLowerCase()}`;
    let url = fileType === 'CSV' ? '/Expense/CSVExport' : '/Expense/PdfExport';

    fetch(url, {
        method: 'GET',
    })
        .then(response => response.blob())
        .then(blob => {
            let link = document.createElement('a');
            link.href = window.URL.createObjectURL(blob);
            link.download = fileName;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
            loader.hide();
        })
        .catch(error => {
            toastr.error('Error downloading file: ' + error.message);
            loader.hide(); 
        })
        .finally(() => {
            loader.hide(); 
        });
}
//#endregion
//#region Datefilter 
function applyFilter(type) {
    let filterInput = $("#dateFilter");
    let filterText = $("#filterText");
    let hideDaterange = $(".daterangepicker");
    if (filterInput.data("datepicker")) {
        filterInput.datepicker("destroy");
    }
    if (filterInput.data("daterangepicker")) {
        filterInput.daterangepicker("remove");
        filterInput.off(".daterangepicker");
    }
    filterInput.val("");
    filterInput.hide();

    switch (type) {
        case 'date':
            filterText.text("Filter by Date");
            filterInput.show().datepicker({
                format: "mm-dd-yyyy",
                autoclose: true,
                todayHighlight: true
            });
            break;
        case 'daterange':

            filterText.text("Filter by Date Range");
            filterInput.show().daterangepicker({
                autoUpdateInput: false,
                locale: { format: "MM-DD-YYYY" }
            }).on('apply.daterangepicker', function (ev, picker) {
                let formattedValue = picker.startDate.format('MM-DD-YYYY') + ' - ' + picker.endDate.format('MM-DD-YYYY');
                $(this).val(formattedValue);
                $("#dateFilter").val(formattedValue).trigger('change'); 
            });
            break;

        case 'month':
            filterText.text("Filter by Month");
            filterInput.show().datepicker({
                format: "mm-yyyy",
                viewMode: "months",
                minViewMode: "months",
                autoclose: true
            });
            break;

        case 'year':
            filterText.text("Filter by Year");
            filterInput.show().datepicker({
                format: "yyyy",
                viewMode: "years",
                minViewMode: "years",
                autoclose: true
            });
            break;
    }
}


//#endregion
