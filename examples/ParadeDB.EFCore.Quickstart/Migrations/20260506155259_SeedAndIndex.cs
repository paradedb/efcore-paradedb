using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParadeDB.EFCore.Quickstart.Migrations
{
    /// <inheritdoc />
    public partial class SeedAndIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                CALL paradedb.create_bm25_test_table(
                    schema_name => 'public', table_name => 'mock_items_temp');
                """
            );

            migrationBuilder.Sql(
                """
                INSERT INTO mock_items (id, description, category, rating, in_stock, created_at, metadata)
                SELECT id, description, category, rating, in_stock, created_at, metadata
                FROM mock_items_temp;
                """
            );

            migrationBuilder.Sql("DROP TABLE mock_items_temp;");

            migrationBuilder.Sql(
                """
                CREATE INDEX mock_items_bm25_idx ON mock_items
                USING bm25 (
                    id,
                    description,
                    rating,
                    (category::pdb.literal),
                    (metadata::pdb.unicode_words('columnar=true'))
                )
                WITH (key_field = 'id');
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS mock_items_bm25_idx;");
        }
    }
}
