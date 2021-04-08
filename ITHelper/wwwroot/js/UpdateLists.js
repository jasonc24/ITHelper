function refreshCategoryList() {
    var parentCategoryList = document.getElementById("ParentCategory");
    var parentCategory = parentCategoryList.value;

    $.ajax({
        cache: false,
        type: "GET",
        traditional: true,
        url: "/ITHelper/Tickets/GetSubCategories",
        data: { "parentCategory": parentCategory },
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        async: true,
        success: function (x) {
            var categoryList = JSON.parse(x);

            var innerHTML = "<select id='CategoryId' name='CategoryId' class='form-control' >";
            var optionBase = "<option value='{0}' {1}>{2}</option>";

            for (let item of categoryList) {
                var option = optionBase.replace("{0}", item.Value);
                option = option.replace("{1}", "");
                option = option.replace("{2}", item.Text);
                innerHTML += option;
            }
            innerHTML += "</select>";
;
            var codeCB = document.getElementById("categoryDiv");
            codeCB.innerHTML = innerHTML;
        },
        error: function (xhr, ajaxOptions, thrownError) { alert("Error retrieving updated codes list...  Please try again.\n" + thrownError.toString()); },
    });
}