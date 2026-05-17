using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParadeDB.EFCore.IntegrationTests.Persistence.Migrations
{
    public partial class CreateBM25Index : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"CREATE INDEX search_idx_product ON ""products""
                  USING bm25 (""id"", ""description"")
                  WITH (key_field='id');"
            );

            migrationBuilder.Sql(
                @"CREATE INDEX search_idx_item ON ""items""
                  USING bm25 (
                    ""id"",
                    (""description""::pdb.literal),
                    (""description""::pdb.simple('alias=description_simple'))
                  )
                  WITH (key_field='id');"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS search_idx_product");
            migrationBuilder.Sql(@"DROP INDEX IF EXISTS search_idx_item");
        }
    }
}
