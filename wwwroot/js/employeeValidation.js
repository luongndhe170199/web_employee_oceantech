// Lấy dữ liệu từ ViewBag đã được truyền vào từ file View
var districts = @Html.Raw(ViewBag.DistrictsJson);
var communes = @Html.Raw(ViewBag.CommunesJson);

console.log(districts); // Kiểm tra dữ liệu quận/huyện
console.log(communes);  // Kiểm tra dữ liệu xã/phường

$('#ProvinceId').change(function () {
    var selectedProvinceId = $(this).val();
    console.log("Selected ProvinceId:", selectedProvinceId);

    var filteredDistricts = districts.filter(function (district) {
        return district.ProvinceId.toString() == selectedProvinceId.toString();
    });

    $('#DistrictId').empty().append('<option selected>Chọn Quận/Huyện</option>');
    filteredDistricts.forEach(function (district) {
        $('#DistrictId').append('<option value="' + district.DistrictId + '">' + district.DistrictName + '</option>');
    });

    $('#CommuneId').empty().append('<option selected>Chọn Xã/Phường</option>');
});

$('#DistrictId').change(function () {
    var selectedDistrictId = $(this).val();
    console.log("Selected DistrictId:", selectedDistrictId);

    var filteredCommunes = communes.filter(function (commune) {
        return commune.DistrictId.toString() == selectedDistrictId.toString();
    });

    $('#CommuneId').empty().append('<option selected>Chọn Xã/Phường</option>');
    filteredCommunes.forEach(function (commune) {
        $('#CommuneId').append('<option value="' + commune.CommuneId + '">' + commune.CommuneName + '</option>');
    });
});

// Validate form
$('form').submit(function (event) {
    var isValid = true;
    var errorMessage = '';

    // Validate FullName
    var fullName = $('#FullName').val().trim();
    if (fullName === '' || fullName.length > 100) {
        isValid = false;
        errorMessage += 'Họ tên không được để trống và không quá 100 kí tự.\n';
    }

    // Validate BirthDate
    var birthDate = new Date($('#BirthDate').val());
    var today = new Date();
    if (birthDate > today) {
        isValid = false;
        errorMessage += 'Ngày sinh phải nhỏ hơn hoặc bằng ngày hôm nay.\n';
    }

    // Validate Age
    var age = parseInt($('#Age').val());
    if (age !== (today.getFullYear() - birthDate.getFullYear())) {
        isValid = false;
        errorMessage += 'Tuổi phải bằng với năm hiện tại trừ đi năm sinh.\n';
    }

    // Validate Ethnicity, Occupation, Position
    if ($('#EthnicityId').val() === 'Chọn dân tộc' ||
        $('#OccupationId').val() === 'Chọn nghề nghiệp' ||
        $('#PositionId').val() === 'Chọn chức vụ') {
        isValid = false;
        errorMessage += 'Dân tộc, nghề nghiệp, và chức vụ không được để trống.\n';
    }

    // Validate CitizenId
    var citizenId = $('#CitizenId').val().trim();
    if (citizenId.length < 10 || citizenId.length > 50) {
        isValid = false;
        errorMessage += 'CCCD phải có từ 10 đến 50 kí tự.\n';
    }

    // Validate PhoneNumber
    var phoneNumber = $('#PhoneNumber').val().trim();
    var phonePattern = /^0\d{9,14}$/; // Bắt đầu bằng số 0 và có từ 10 đến 15 ký tự
    if (!phonePattern.test(phoneNumber)) {
        isValid = false;
        errorMessage += 'Số điện thoại phải bắt đầu bằng số 0 và có từ 10 đến 15 kí tự.\n';
    }

    // Show error message and prevent form submission if invalid
    if (!isValid) {
        alert(errorMessage);
        event.preventDefault();
    }
});
