var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url":"/Admin/Product/GetAll"
        },
        "columns": [
            { "data": "title", "width": "15%" },
            { "data": "isbn", "width": "15%" },
            { "data": "price", "width": "15%" },
            { "data": "author", "width": "15%" },
            { "data": "category.name", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                        <a class="btn btn-info" href="/Admin/Product/Upsert?id=${data}">
							<i class="bi bi-pencil-square"></i>
						Update
					    </a>
                        <a class="btn btn-danger">
									<i class="bi bi-trash"></i>
							Delete
						</a>

                    `
                }
                , "width": "15",
                }
            ]
            
        });
}