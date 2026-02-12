Feature: Token Replacement
  As a developer
  I want tokens in templates to be replaced with actual values
  So that generated modules have the correct names, namespaces, and metadata

  Background:
    Given I have a token replacement service
    And I provide the following module options:
      | Field       | Value              |
      | Owner       | MarkDav            |
      | Module      | WeatherArbitrage   |
      | Description | Weather analysis   |
      | Framework   | net10.0            |

  Scenario: Replace standard tokens in content
    Given I have content with "[Owner].[Module]"
    When I replace tokens in the content
    Then the result should be "MarkDav.WeatherArbitrage"

  Scenario: Replace description token
    Given I have content with "Description: [Description]"
    When I replace tokens in the content
    Then the result should be "Description: Weather analysis"

  Scenario: Replace framework token
    Given I have content with "<TargetFramework>[Framework]</TargetFramework>"
    When I replace tokens in the content
    Then the result should be "<TargetFramework>net10.0</TargetFramework>"

  Scenario: Replace year token with current year
    Given I have content with "Copyright [Year]"
    When I replace tokens in the content
    Then the result should contain the current year

  Scenario: Replace date token with current date
    Given I have content with "Generated on [Date]"
    When I replace tokens in the content
    Then the result should contain the current date in format "yyyy-MM-dd"

  Scenario: Generate unique GUID for each occurrence
    Given I have content with "[Guid]" appearing 3 times
    When I replace tokens in the content
    Then the result should contain 3 different GUIDs
    And each GUID should be a valid GUID format

  Scenario: Replace multiple tokens in same content
    Given I have content with "namespace [Owner].[Module] // [Description]"
    When I replace tokens in the content
    Then the result should be "namespace MarkDav.WeatherArbitrage // Weather analysis"

  Scenario: Handle content with no tokens
    Given I have content with "No tokens here"
    When I replace tokens in the content
    Then the result should be "No tokens here"

  Scenario: Replace tokens in file path
    Given I have path "[Owner].[Module].Client/Controllers/[Module]Controller.cs"
    When I replace tokens in the path
    Then the result should be "MarkDav.WeatherArbitrage.Client/Controllers/WeatherArbitrageController.cs"

  Scenario: Replace tokens in directory path
    Given I have path "Modules/[Owner].[Module]/"
    When I replace tokens in the path
    Then the result should be "Modules/MarkDav.WeatherArbitrage/"

  Scenario: Do not replace [Guid] in paths
    Given I have path "[Owner].[Module]/[Guid].txt"
    When I replace tokens in the path
    Then the result should be "MarkDav.WeatherArbitrage/[Guid].txt"
    And the result should still contain "[Guid]"

  Scenario: Handle empty content
    Given I have empty content
    When I replace tokens in the content
    Then the result should be empty

  Scenario: Handle empty path
    Given I have an empty path
    When I replace tokens in the path
    Then the result should be empty

  Scenario: Case-sensitive token matching
    Given I have content with "[owner]" in lowercase
    When I replace tokens in the content
    Then the result should still contain "[owner]"
    And the result should not contain "MarkDav"

  Scenario: Complex namespace replacement
    Given I have content with:
      """
      namespace [Owner].[Module].Client
      {
          public class [Module]Controller
          {
              // Created: [Date]
              // Copyright [Year] [Owner]
              // ID: [Guid]
              // Another ID: [Guid]
          }
      }
      """
    When I replace tokens in the content
    Then the result should contain "namespace MarkDav.WeatherArbitrage.Client"
    And the result should contain "public class WeatherArbitrageController"
    And the result should contain the current year
    And the result should contain the current date
    And the result should contain 2 different GUIDs
