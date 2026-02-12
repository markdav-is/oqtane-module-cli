Feature: Template Manifest Parsing
  As a developer
  I want the CLI to read and parse template manifests
  So that I know what files to generate and which tokens to replace

  Background:
    Given I am in an Oqtane solution directory
    And there is a template at "Oqtane.Server/wwwroot/Modules/Templates/Internal"

  Scenario: Parse valid template manifest
    Given the template has a valid "template.json" file with:
      """
      {
        "name": "Internal Module Template",
        "version": "1.0.0",
        "owner": "Default",
        "module": "Template",
        "description": "Standard internal Oqtane module",
        "files": [
          {
            "path": "[Owner].[Module].Client/Module.cs",
            "tokens": ["[Owner]", "[Module]", "[Description]"]
          },
          {
            "path": "[Owner].[Module].Server/Controllers/[Module]Controller.cs",
            "tokens": ["[Owner]", "[Module]", "[Guid]"]
          }
        ]
      }
      """
    When I load the template manifest
    Then the manifest name should be "Internal Module Template"
    And the manifest version should be "1.0.0"
    And the manifest should have 2 files
    And the first file path should be "[Owner].[Module].Client/Module.cs"
    And the first file should have 3 tokens

  Scenario: Handle missing template.json file
    Given the template directory exists
    But there is no "template.json" file
    When I attempt to load the template manifest
    Then it should throw a FileNotFoundException
    And the error message should contain "Template manifest not found"

  Scenario: Handle invalid JSON in manifest
    Given the template has a "template.json" file with invalid JSON
    When I attempt to load the template manifest
    Then it should throw an InvalidOperationException
    And the error message should contain "Invalid template manifest"

  Scenario: Handle null deserialization result
    Given the template has a "template.json" file that deserializes to null
    When I attempt to load the template manifest
    Then it should throw an InvalidOperationException
    And the error message should contain "deserialized to null"

  Scenario: Parse manifest with no files
    Given the template has a valid "template.json" file with:
      """
      {
        "name": "Empty Template",
        "version": "1.0.0",
        "owner": "Default",
        "module": "Template",
        "description": "Template with no files",
        "files": []
      }
      """
    When I load the template manifest
    Then the manifest should have 0 files

  Scenario: Parse manifest with file tokens
    Given the template has a valid "template.json" file
    And a file entry has tokens: ["[Owner]", "[Module]", "[Guid]", "[Year]", "[Date]", "[Framework]", "[Description]"]
    When I load the template manifest
    Then the file should have all 7 tokens listed

  Scenario: List files to be generated during module creation
    Given I run the command "module create --owner Acme --name Blog"
    When the CLI loads the template manifest
    Then the output should show "Files to be created:"
    And each file path should have tokens replaced with actual values
    And "[Owner]" should be replaced with "Acme"
    And "[Module]" should be replaced with "Blog"
