Feature: Template Discovery
  As a developer
  I want the CLI to discover and list available templates
  So that I can see what module types I can create

  Background:
    Given I am in an Oqtane solution directory
    And the solution has an "internal" template
    And the solution has an "external" template

  Scenario: List all available templates
    When I run the command "module list"
    Then the command should succeed
    And the output should contain "Available Module Templates:"
    And the output should contain "internal"
    And the output should contain "external"

  Scenario: List templates from a specific solution path
    Given I am outside the Oqtane solution directory
    When I run the command "module list --solution <solution-path>"
    Then the command should succeed
    And the output should contain "Available Module Templates:"

  Scenario: Handle missing solution directory
    Given I am outside the Oqtane solution directory
    When I run the command "module list"
    Then the command should fail
    And the error output should contain "Cannot find Oqtane solution"

  Scenario: Handle no templates found
    Given I am in an Oqtane solution directory
    And the solution has no templates
    When I run the command "module list"
    Then the command should succeed
    And the output should contain "No module templates found"

  Scenario: Find solution by walking up directory tree
    Given I am in a subdirectory of the Oqtane solution
    When I run the command "module list"
    Then the command should succeed
    And the output should contain "Available Module Templates:"

  Scenario: Recognize both .sln and .slnx files
    Given I am in a directory with a ".slnx" file
    When I run the command "module list"
    Then the command should succeed

  Scenario: Skip templates with invalid manifests
    Given I am in an Oqtane solution directory
    And the solution has an "internal" template with an invalid manifest
    And the solution has an "external" template
    When I run the command "module list"
    Then the command should succeed
    And the output should contain "external"
    And the output should not contain "internal"
