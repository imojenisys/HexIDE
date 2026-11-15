# Skipped tests in the .NET 11 spike

Every skipped test is listed below; none is cited as coverage. The clean-worktree IDE runs have identical skip sets.

## Runtime

- `HexIDE.Runtime.Tests.ContainerHierarchySaveTests.RoundTrip_OfVb6sOwnNestedTemplates_ReproducesTheBeginStructure`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.The_registry_keeps_each_setting_as_text_as_vb6_exe_does`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.VbSettingsStoreTests.A_value_another_program_wrote_that_is_not_text_reads_as_no_text`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_renamed_module_retargets_VB_Name_but_keeps_everything_else`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.Instancing_and_data_binding_settings_survive`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.ClassHeaderPreservationTests.A_real_vb6_class_round_trips_byte_for_byte`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.DateFractionOfASecondTests.Long_date_uses_the_measured_Windows_pattern`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.Vb6OracleRoundTripTests.HexIDE_output_still_builds_in_vb6`: Set HEXIDE_ORACLE=1 to compile round-tripped forms with VB6.
- `HexIDE.Runtime.Tests.Vb6TextFileTests.The_one_corpus_file_with_high_bytes_round_trips`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.UiThreadTests.Child_ReportsTheUiThreadsStack`: Runs only as the child of OnLinux_TheUiThreadsStack_IsTheSameWhateverUlimitSays
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Era_date_output_matches_GetDateFormatEx_and_reads_without_a_wrong_date`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.WindowsDateLocaleTests.Date_output_matches_GetDateFormatEx`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.RoundTrip_OfVb6sOwnMenuTemplates_ReproducesTheMenuShape`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.MenuHierarchySaveTests.Vb6sOwnMenuTemplates_AreNoLongerHeldReadOnly`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_attribute_rows`: Windows-only: this pins behaviour the product deliberately varies by host.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_numeric_rows`: Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.FileSystemTests.CurDir_uses_only_the_first_drive_character`: Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_error_rows`: Windows filesystem attributes required.
- `HexIDE.Runtime.Tests.FileSystemTests.Windows_name_rows`: Windows filesystem attributes required.

## TwinBasic

- `HexIDE.TwinBasic.Tests.LiveCompilerTests.Root_initialization_and_shutdown_are_recorded`: The twinBASIC compiler runs on Windows only.
- `HexIDE.TwinBasic.Tests.LiveCompilerTests.A_compiler_started_without_the_IDE_loads_a_project_attaches_every_channel_and_shuts_down`: The twinBASIC compiler runs on Windows only.

## HexIDE.Tests

- `HexIDE.Tests.Projects.DuplicateProjectOpenTests.Opening_the_same_project_with_different_drive_and_folder_case_is_refused`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.LspClient.LspDocumentSessionTests.AWindowsDriveLetterInTheOtherCaseStillMatches`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "brand.json")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "branding/logo.png")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.sig")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "HexIDE.exe")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "nested/HEXIDE-FIRSTPARTY.PUB")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "acme-ide.exe")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations.json")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations-cache.json")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "LICENSE")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "hexide-root.pub")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_protected_files_are_refused_without_output(relative: "revocations.sig")`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Every_file_but_the_executable_is_exactly_as_published`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_default_profile_folder_travels_with_the_brand`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_installer_inputs_are_written_beside_the_build_and_not_inside_it`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_branded_process_declares_that_identity_to_Windows`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_repacked_executable_says_it_is_the_brand_when_Windows_reads_it`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_folder_below_a_link_is_allowed`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_icon_is_the_brands_with_nothing_left_of_stocks`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_output_ancestor_is_refused_before_staging`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_links_are_refused_at_the_root_or_inside`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_missing_folder_is_refused`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.The_brands_files_land_where_the_IDE_will_look_for_them`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_run_that_fails_partway_leaves_nothing_behind`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_signed_stock_executable_comes_out_unsigned_and_the_run_says_so`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.Included_files_reach_the_installable_payload_and_report`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Repack.WindowsRepackerTests.A_brand_with_no_copyright_keeps_stocks_rather_than_claiming_none`: Needs a Windows x64 test host, whose launcher stands in for a stock executable.
- `HexIDE.Tests.Addins.PackageVerificationTests.Locked_signature_file_yields_Untrusted_not_an_exception`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Saving_a_project_leaves_an_untouched_class_byte_identical`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.A_reload_from_disk_also_preserves_the_header`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.ClassModuleSaveFidelityTests.Editing_the_body_leaves_the_header_untouched`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.LspClient.WorkspaceArtifactClangdTests.ClangdReadsGeneratedCompilationArguments`: No 'cpp' language server available. It is normally downloaded on demand; set HEXIDE_CPP_LSP to an executable, or put `clangd` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.CrossDriveMember_RendersAtRoot`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.AMemberWithNoRelativeForm_CaptionsItsAbsoluteLocation`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Tools.ProjectTreeBuilderTests.ForwardSlashPath_NormalizesAndNests`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Projects.ContributedProjectTests.A_contributed_project_already_open_is_refused_when_opened_again_with_different_drive_and_folder_case`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.UserDataPathTests.OnWindowsItIsUnderAppData`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Projects.MadeProgramTests.A_made_program_starts_and_runs_its_Sub_Main`: A made program is started on Windows only: elsewhere it needs a desktop session.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_program_started_hidden_is_read_as_hidden_and_the_note_says_vbHide_did_it`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_minimized_program_is_read_as_minimized_and_visible`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_window_of_a_process_the_started_one_started_is_read_through_it`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Server.ProcessWindowsToolTests.A_running_process_with_no_window_is_said_to_have_none_yet`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionEscapeIsRefused`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Profiles.ProfileContainmentTests.JunctionSwappedInAfterPreflightIsRefusedDuringRead`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.TheTypeLibraryNamesOleAutomationStdole`: Type libraries are read only on Windows.
- `HexIDE.Tests.LspClient.WorkspaceArtifactProjectTests.ALocalTypeLibraryIsNamedFromItsFile`: Type libraries are read only on Windows.
- `HexIDE.Tests.LspClient.ForeignServerIntegrationTests.ItAdvertisesCapabilitiesAndTheyAreVisibleOnTheConnection`: No 'markdown' language server available. It is normally downloaded on demand; set HEXIDE_MARKDOWN_LSP to an executable, or put `rumdl` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.ForeignServerIntegrationTests.TheRecordSaysWhenTheHandshakesBytesReachedTheServerAndCameBack`: No 'markdown' language server available. It is normally downloaded on demand; set HEXIDE_MARKDOWN_LSP to an executable, or put `rumdl` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.ForeignServerIntegrationTests.AVb6DocumentIsNotRoutedToTheMarkdownServer`: No 'markdown' language server available. It is normally downloaded on demand; set HEXIDE_MARKDOWN_LSP to an executable, or put `rumdl` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.ForeignServerIntegrationTests.UnpromptedInboundMethodsAreAccountedForAndProgressIsDelivered`: No 'markdown' language server available. It is normally downloaded on demand; set HEXIDE_MARKDOWN_LSP to an executable, or put `rumdl` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.ForeignServerIntegrationTests.TellingAForeignServerADocumentWasSavedMakesItLookAgain`: No 'markdown' language server available. It is normally downloaded on demand; set HEXIDE_MARKDOWN_LSP to an executable, or put `rumdl` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.ForeignServerIntegrationTests.AFeatureTheForeignServerDoesNotAdvertiseIsNotRequested`: No 'markdown' language server available. It is normally downloaded on demand; set HEXIDE_MARKDOWN_LSP to an executable, or put `rumdl` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.ForeignServerIntegrationTests.AForeignServerStartsLazilyAndItsDiagnosticsReachUs`: No 'markdown' language server available. It is normally downloaded on demand; set HEXIDE_MARKDOWN_LSP to an executable, or put `rumdl` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.ForeignServerIntegrationTests.AServerAttachedOnlyByAConfigurationFileAnswersForReal`: No 'markdown' language server available. It is normally downloaded on demand; set HEXIDE_MARKDOWN_LSP to an executable, or put `rumdl` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.LineFoldingNegotiationTests.AServerEndsAFoldOnTheClosingBraceLine`: No 'cpp' language server available. It is normally downloaded on demand; set HEXIDE_CPP_LSP to an executable, or put `clangd` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.ProtocolEnumCoverageTests.WireEnumsCoverTheSpecificationValues`: The LSP metaModel is unavailable. It is normally downloaded on demand; set HEXIDE_LSP_METAMODEL to a local copy to work offline.
- `HexIDE.Tests.LspClient.TwoForeignServersTests.ItAdvertisesItsOwnCapabilitiesRatherThanTheOtherServersHabits`: No 'latex' language server available. It is normally downloaded on demand; set HEXIDE_LATEX_LSP to an executable, or put `texlab` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.TwoForeignServersTests.ASecondServerByAnotherAuthorAlsoWorks`: No 'latex' language server available. It is normally downloaded on demand; set HEXIDE_LATEX_LSP to an executable, or put `texlab` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.TwoForeignServersTests.EachServerIsOfferedOnlyTheDocumentsItClaims`: No 'latex' language server available. It is normally downloaded on demand; set HEXIDE_LATEX_LSP to an executable, or put `texlab` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.TwoForeignServersTests.ALatexServerClaimingClsIsNotOfferedVb6Modules`: No 'latex' language server available. It is normally downloaded on demand; set HEXIDE_LATEX_LSP to an executable, or put `texlab` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.TwoForeignServersTests.ACarriedLatexClassIsStillOfferedToTheLatexServer`: No 'latex' language server available. It is normally downloaded on demand; set HEXIDE_LATEX_LSP to an executable, or put `texlab` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.DebugAdapter.ForeignAdapterTests.Debugpy_runs_a_complete_session`: debugpy 1.8.22 is not cached; HEXIDE_FOREIGN_LSP_DOWNLOAD disables acquisition.
- `HexIDE.Tests.DebugAdapter.ForeignAdapterTests.Netcoredbg_runs_a_complete_session`: netcoredbg 3.2.0-1092 is not cached; HEXIDE_FOREIGN_LSP_DOWNLOAD disables acquisition.
- `HexIDE.Tests.Repack.LinuxRepackerTests.A_linux_build_is_refused_on_windows`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.IDE.EditorServiceNavigationTests.AFileUriWhoseDriveLetterTheServerNormalisedStillResolves`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.LspClient.ReferenceImplementationTests.ItAdvertisesCapabilitiesWeCanRead`: No 'json' language server available. It is normally downloaded on demand; set HEXIDE_JSON_LSP to an executable, or put `vscode-json-language-server` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.ReferenceImplementationTests.ItAcceptsOurHandshakeAndAnswersAboutADocument`: No 'json' language server available. It is normally downloaded on demand; set HEXIDE_JSON_LSP to an executable, or put `vscode-json-language-server` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.ReferenceImplementationTests.EditingADocumentUpdatesWhatItReports`: No 'json' language server available. It is normally downloaded on demand; set HEXIDE_JSON_LSP to an executable, or put `vscode-json-language-server` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_process_left_out_of_the_job_outlives_it`: Job objects exist only on Windows.
- `HexIDE.Tests.Processes.ChildProcessJobTests.A_closed_job_refuses_a_process`: Job objects exist only on Windows.
- `HexIDE.Tests.Processes.ChildProcessJobTests.Closing_the_job_ends_the_process_put_in_it`: Job objects exist only on Windows.
- `HexIDE.Tests.LspClient.DeclarationIsNotDefinitionTests.TheDeclarationGateStaysShutForAServerThatDoesNotOfferIt`: No 'cpp' language server available. It is normally downloaded on demand; set HEXIDE_CPP_LSP to an executable, or put `clangd` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.DeclarationIsNotDefinitionTests.AServerAnswersDeclarationAndDefinitionDifferently`: No 'cpp' language server available. It is normally downloaded on demand; set HEXIDE_CPP_LSP to an executable, or put `clangd` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.HierarchicalSymbolsNegotiationTests.AServerThatWaitsToBeToldSendsTheOutlineAsATree`: No 'cpp' language server available. It is normally downloaded on demand; set HEXIDE_CPP_LSP to an executable, or put `clangd` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.RealServerShutdownTests.JsonClosesAndExits`: No 'json' language server available. It is normally downloaded on demand; set HEXIDE_JSON_LSP to an executable, or put `vscode-json-language-server` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.RealServerShutdownTests.PythonClosesAndExits`: No 'python' language server available. It is normally downloaded on demand; set HEXIDE_PYTHON_LSP to an executable, or put `ruff` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.RealServerShutdownTests.LatexClosesAndExits`: No 'latex' language server available. It is normally downloaded on demand; set HEXIDE_LATEX_LSP to an executable, or put `texlab` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.RealServerShutdownTests.MarkdownClosesAndExits`: No 'markdown' language server available. It is normally downloaded on demand; set HEXIDE_MARKDOWN_LSP to an executable, or put `rumdl` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.RealServerShutdownTests.CppClosesAndExits`: No 'cpp' language server available. It is normally downloaded on demand; set HEXIDE_CPP_LSP to an executable, or put `clangd` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.Infrastructure.WindowStateUnconfirmedTests.On_Windows_the_probe_does_not_guess`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.DebugAdapter.DapProtocolCoverageDocTests.CoverageMatchesSchemaAndClientCode`: DAP schema unavailable; set HEXIDE_DAP_SCHEMA to the pinned schema for offline use. 
- `HexIDE.Tests.LspClient.CompletionForeignTests.ClangdSnippetInsertsArgumentsAndNavigatesStops`: No clangd available; RequiredServersTests enforces availability in CI.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_second_save_still_leaves_the_companion_intact`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.AFormWhoseBlobsAreAllModelled_DoesNotRaiseTheLossFlag`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_that_reproduces_its_companion_is_never_denied_the_write`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.A_form_whose_records_HexIDE_does_not_model_saves_both_halves_as_VB6_wrote_them`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Dropping_a_blob_bearing_control_keeps_both_halves_in_step`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.The_form_records_that_it_lost_binary_fidelity`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.Projects.CompanionBinaryPreservationTests.Saving_never_shrinks_or_deletes_a_companion_it_cannot_reproduce`: No VB6 Template tree here: VB6_TEMPLATES is not set, and VB6 is not installed at 'C:\\Program Files (x86)\\Microsoft Visual Studio\\VB98\\Template'.
- `HexIDE.Tests.LspClient.PullDiagnosticsTests.DeclaringThePullModelStopsTheServerPublishing`: No 'python' language server available. It is normally downloaded on demand; set HEXIDE_PYTHON_LSP to an executable, or put `ruff` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.PullDiagnosticsTests.ItAdvertisesThePullModelInTheShapeThatOnceBrokeTheHandshake`: No 'python' language server available. It is normally downloaded on demand; set HEXIDE_PYTHON_LSP to an executable, or put `ruff` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.WatchedFilesForeignTests.RuffRereadsPyprojectAfterAnExternalEdit`: No 'python' language server available. It is normally downloaded on demand; set HEXIDE_PYTHON_LSP to an executable, or put `ruff` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.Infrastructure.ProjectWriteSafetyTests.A_copy_that_cannot_be_removed_is_kept_and_named`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Sidecar.UserSidecarSaveTests.TheProjectsOwnSidecarSpelledInAnotherCaseIsStillItsOwn`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.MacRepackerTests.A_signature_macOS_rejects_stops_the_run_and_leaves_nothing`: macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_repacked_as_one`: macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.Repack.MacRepackerTests.A_mac_build_is_refused_on_windows`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.Repack.MacRepackerTests.macOS_accepts_the_property_list_signature_and_icon`: macOS-only: asks macOS's own tools about the result.
- `HexIDE.Tests.Profiles.ProfileReaderSafetyTests.WindowsPipeHandleIsRefused`: Windows-only: this pins behaviour the product deliberately varies by host filesystem.
- `HexIDE.Tests.LspClient.ProtocolCoverageDocTests.EverySectionHeadingMatchesItsOwnRows`: The LSP metaModel is unavailable. It is normally downloaded on demand; set HEXIDE_LSP_METAMODEL to a local copy to work offline.
- `HexIDE.Tests.LspClient.ProtocolCoverageDocTests.EveryRowRecordsTheDirectionTheSpecificationGivesIt`: The LSP metaModel is unavailable. It is normally downloaded on demand; set HEXIDE_LSP_METAMODEL to a local copy to work offline.
- `HexIDE.Tests.LspClient.ProtocolCoverageDocTests.TheHeadlineCountsMatchTheTableBeneathThem`: The LSP metaModel is unavailable. It is normally downloaded on demand; set HEXIDE_LSP_METAMODEL to a local copy to work offline.
- `HexIDE.Tests.LspClient.ProtocolCoverageDocTests.TheTableListsEveryMessageTheProtocolDefines`: The LSP metaModel is unavailable. It is normally downloaded on demand; set HEXIDE_LSP_METAMODEL to a local copy to work offline.
- `HexIDE.Tests.LspClient.ProtocolCoverageDocTests.TheTableInventsNothingTheProtocolDoesNotDefine`: The LSP metaModel is unavailable. It is normally downloaded on demand; set HEXIDE_LSP_METAMODEL to a local copy to work offline.
- `HexIDE.Tests.LspClient.ProtocolCoverageDocTests.EachMessageAppearsExactlyOnce`: The LSP metaModel is unavailable. It is normally downloaded on demand; set HEXIDE_LSP_METAMODEL to a local copy to work offline.
- `HexIDE.Tests.LspClient.TypeLibraryNamesTests.ALocalLinkToAShareIsNotALocalPath`: A link to a share is a Windows path.
- `HexIDE.Tests.LspClient.UntitledDocumentNamesTests.APullModelServerAnswersAboutADocumentWithNoFile`: No 'python' language server available. It is normally downloaded on demand; set HEXIDE_PYTHON_LSP to an executable, or put `ruff` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.UntitledDocumentNamesTests.TheReferenceImplementationAnswersAboutADocumentWithNoFile`: No 'json' language server available. It is normally downloaded on demand; set HEXIDE_JSON_LSP to an executable, or put `vscode-json-language-server` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.UntitledDocumentNamesTests.AMarkdownServerAnswersAboutADocumentWithNoFile`: No 'markdown' language server available. It is normally downloaded on demand; set HEXIDE_MARKDOWN_LSP to an executable, or put `rumdl` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.UntitledDocumentNamesTests.WatchedFileRegistrationDoesNotTalkThisServerOutOfAnswering`: No 'python' language server available. It is normally downloaded on demand; set HEXIDE_PYTHON_LSP to an executable, or put `ruff` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.UntitledDocumentNamesTests.TheServerThatAcceptsOnlyFileUrisSaysSoOnStandardError`: No 'cpp' language server available. It is normally downloaded on demand; set HEXIDE_CPP_LSP to an executable, or put `clangd` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.UntitledDocumentNamesTests.ANameThatIsNotPercentEncodedIsDroppedWhereItsEncodedFormIsAnswered`: No 'latex' language server available. It is normally downloaded on demand; set HEXIDE_LATEX_LSP to an executable, or put `texlab` on PATH, to choose your own. Offline machines skip these.
- `HexIDE.Tests.LspClient.UntitledDocumentNamesTests.ALatexServerAnswersAboutADocumentWithNoFile`: No 'latex' language server available. It is normally downloaded on demand; set HEXIDE_LATEX_LSP to an executable, or put `texlab` on PATH, to choose your own. Offline machines skip these.

## Integration

- `HexIDE.Integration.Tests.Views.ContextMenuMeasureTests.CodeMenuFitsItsShortcutsAndSubmenuArrowsOnFirstOpening`: Needs Avalonia 12.1.5; pinned back to 12.1.4 for #3869
- `HexIDE.Integration.Tests.Automation.UiAutomationDriverTests.Interact_Expand_LeafNode_RefusesClearly`: Needs Avalonia 12.1.5; pinned back to 12.1.4 for #3869
- `HexIDE.Integration.Tests.Automation.UiAutomationDriverTests.Inspect_TreeNode_ReportsExpandCollapseState`: Needs Avalonia 12.1.5; pinned back to 12.1.4 for #3869
- `HexIDE.Integration.Tests.Automation.UiAutomationDriverTests.Interact_Collapse_LeafNode_RefusesClearly`: Needs Avalonia 12.1.5; pinned back to 12.1.4 for #3869

## Runtime.Integration

- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Segoe_UI_nine_point_matches_console_measurements`: Requires the oracle's Windows Segoe UI font build
- `HexIDE.Runtime.Integration.Tests.Views.ToolbarCaptionTests.Windows_caption_width_follows_installed_fonts_Gdi_extent`: Requires Windows GDI

## LspServer


