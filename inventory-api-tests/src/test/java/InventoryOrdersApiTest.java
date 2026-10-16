import io.restassured.RestAssured;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;

import static io.restassured.RestAssured.given;

public class InventoryOrdersApiTest {
    @BeforeAll
    static void setup() {
        RestAssured.baseURI = System.getProperty("api.baseurl", "http://localhost:5162");
    }

    @Test
    void getProductsShouldReturnSuccess() {
        given()
                .when()
                .get("/api/products")
                .then()
                .statusCode(200);
    }

    @Test
    void getMissingProduct_ReturnsNotFound() {
        given().when().get("/api/Products/2147483647").then().statusCode(404);
    }

    @Test
    void getOrders_ReturnsSuccess() {
        given().when().get("/api/Orders").then().statusCode(200);
    }

    @Test
    void getMissingOrder_ReturnsNotFound() {
        given()
                .when()
                .get("/api/Orders/2147483647")
                .then()
                .statusCode(404);
    }
}