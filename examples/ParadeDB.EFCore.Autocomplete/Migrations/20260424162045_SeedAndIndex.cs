using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ParadeDB.EFCore.Autocomplete.Migrations
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
                    schema_name => 'public',
                    table_name  => 'mock_items_temp'
                );
                """
            );

            migrationBuilder.Sql(
                """
                INSERT INTO autocomplete_items (id, description, category, rating, in_stock, created_at)
                SELECT id, description, category, rating, in_stock, created_at
                FROM mock_items_temp;
                """
            );

            migrationBuilder.Sql("DROP TABLE IF EXISTS mock_items_temp;");

            migrationBuilder.Sql(
                """
                CREATE INDEX autocomplete_items_idx ON autocomplete_items
                USING bm25 (
                    id,
                    (description::pdb.unicode_words),
                    (description::pdb.ngram(3, 8, 'alias=description_ngram')),
                    category
                )
                WITH (key_field = 'id');
                """
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS autocomplete_items_idx;");
        }
    }
}
