# Tasks

## 0. Measurements the rest depends on

- [x] 0.1 The running IDE shows a form's leading `Attribute VB_*` block (measured 2026-09-20 on
  `demo/bill-of-fare`, form `frmBillOfFare`, freshly launched and untouched). `get_file_content` returns a
  buffer opening on the five `Attribute VB_*` lines, and a snapshot of the code window shows them as its
  lines 1-5: syntax-coloured as ordinary code, not folded, not greyed, caret at Ln 1 Col 1. The code was
  right and the comments were wrong; the three that said the block is invisible are corrected. **So, for
  phases 3 and 4:** a form kind's composed prefix is the designer part alone, and so is its sidecar shift —
  the attribute run is already in the buffer and must not be counted twice. Also found while measuring:
  `get_file_content`'s `hasUnsavedChanges` is hardcoded per branch and reports provenance, not dirtiness
  (hexide-io/HexIDE#481); out of scope here.
- [x] 0.2 Both grammars now parse whole files, and did not before (2026-09-20). `WholeFileGrammarTests` in
  each half parses every corpus file whole and classifies three ways: an error in the prefix, an error that
  appears in the body only with the prefix in front of it, and a body that already fails alone. The first two
  block; the third is recorded. **30 of 46 files failed, in both grammars, at the same line of the same
  file** — one cause: VB6 decodes an enumerated property value into a comment when it saves
  (`MultiUse = -1  'True`), `COMMENT` is hidden, and the whitespace in front of it reached rules that went
  straight from the value to `NEWLINE`. Every `.cls` VB6 ever wrote failed at line 3. Fixed with a `WS?` in
  `moduleConfigElement` and `cp_SingleProperty` in both grammars; recorded as fix 7 in
  `docs/vb6-grammar-fixes.md`. Now 0 blocking over 48 files (7 `.bas`, 13 `.cls`, 3 `.ctl`, 24 `.frm`,
  1 `.pag`). **So 3.6 is unblocked: the composed buffer parses.**
  - The `.pag` and one `.ctl` are new, in `corpus/designer/`, with a `.vbp` that `vb6.exe` builds clean.
    Authored here and compiler-validated, **not written by the VB6 IDE** — driving the IDE needs an
    interactive session in the VM, and PowerShell Direct lands in session 0 with no desktop (measured).
    `corpus/designer/README.md` states the provenance and what it does not cover.
  - Two bodies fail on their own, and both are now named rather than left in a report: spring-tide's module
    is deliberately invalid, and `demo/neon-aurora/Class4.cls` line 174 is `1E+30`, which the server's
    grammar rejects and the interpreter's accepts (hexide-io/HexIDE#482).
  - Found on the way, because the new corpus project is the first with a UserControl or PropertyPage in it:
    HexIDE wrote `UserControl=Name; File` into every `.vbp`, which VB6 reads as a filename, so every such
    project it saved was unopenable (hexide-io/HexIDE#483). Fixed, with the five-key rule measured and
    recorded in the oracle; two existing tests had pinned the wrong shape.
- [x] 0.3 Oracle, recorded in `docs/vb6-fidelity-oracle.md` (2026-09-19): the `/out` log reads `Compile Error
  in File '<absolute path>', Line <N> : <message>`, with `N` a 0-based index into the code view — designer
  block and **every** `Attribute` line excluded, procedure-level ones included, physical lines counted. Two
  projects in one group may not share a name (the group is refused and nothing builds); a form and a module
  in one project may not share a name. Both comparisons are case-insensitive.
- [x] 0.4 Foreign servers and `untitled:` — measured against all five and kept as
  `UntitledDocumentNamesTests` (2026-09-20). Each server is asked with **its own** extension, because routing
  is by extension and none of the five claims a VB6 one: what is under test is the scheme, and sending `.bas`
  would test something else. Measured, HexIDE's own client, pinned versions:

  | Server | `untitled:Project1/Module1.<ext>` | leading slash | raw non-ASCII | percent-encoded | delivery |
  |---|---|---|---|---|---|
  | rumdl 0.2.64 | accepted | accepted | accepted | accepted | pull **and** publishes |
  | texlab 5.26.0 | accepted | accepted | **dropped in silence** | accepted | push |
  | vscode-json-language-server | accepted | accepted | accepted | accepted | pull |
  | ruff 0.16.7 | accepted | accepted | accepted | accepted | pull |
  | clangd 22.1.6 | **refused** | refused | refused | refused | push |

  - **Every assertion reads a frame, and the first version did not.** It compared
    `PublishDiagnosticsParams.Uri` with the name sent, which is a real echo from a push server and a
    tautology from a pull one: a `DocumentDiagnosticReport` carries no URI, so `ApplyDiagnosticReport`
    raises the event with the client's own string. Three of the five deliver that way, so three of the tests
    were comparing HexIDE with HexIDE. The capture is armed and the bodies are read instead — the sent
    `didOpen`, the sent pull request, and the received publication.
  - The branch is read from the connection's declared `diagnosticProvider`, never from a list in the test. A
    pull server is proved by a `textDocument/diagnostic` naming that URI, answered, with items in the answer
    — findings it could only produce from text it holds under that name. A push server is proved by the URI
    in its publication **and** by never having been asked. Both halves of the second are needed: rumdl
    advertises a provider and publishes anyway, so either alone leaves the branch undetectably reversible —
    measured by inverting it, which stayed green until the second half was added.
  - clangd's refusal is read off its standard error (`only supports 'file' URI scheme`, naming
    `textDocument/didOpen`), never inferred from the absence of diagnostics. It refuses the scheme, not the
    spelling: the percent-encoded form is refused identically.
  - texlab's silent drop and its encoded form are one test, on one server, in one process, with only the
    spelling changed — the negative half alone would assert an absence. Its patience is a multiple of what
    the positive half just took, so a loaded runner scales it rather than defeating it.
  - Found on the way: a request about a document a server discarded is never answered and nothing gives up
    on it, because only `initialize` has a timeout (hexide-io/HexIDE#486); and a foreign server whose
    download fails its pinned digest makes its tests *vanish* rather than fail, because the refusal throws
    from an attribute constructor (hexide-io/HexIDE#487).
  - Every assertion here was checked by breaking it, including the one that matters most: opening the
    document under a name one character different from the one asserted fails all four tolerant tests, pull
    and push alike. That mutation changes what is *sent*; the earlier round only changed what was expected,
    which is exactly why it could not see the tautology above.
- [x] 0.5 AvaloniaEdit 12.0.0 behaviours, read out of the assembly (2026-09-19): the stock read-only provider
  allows insertion at a region's edges and carves read-only text out of a deletion, and both methods are
  `virtual`; binding the editor's read-only property replaces the whole section provider; a fold's
  closed-by-default flag applies only on the manager's first update to a fold created in it, `UpdateFoldings`
  throws on an unsorted list and skips a zero-length fold; undo entries hold absolute offsets, are internal and
  immutable, cannot be rebased, and any push clears the redo stack, while a group can carry a caller's marker
  that identifies the most recent group. The design records what each one settles.

## 1. Identity inside the IDE

- [x] 1.1 `DocumentIdentity` (HexIDE.Core) — the definition compared by reference, carrying its project;
  `Name`, `AbsolutePath` and `Display` read live and nothing keys on any of them. A UserControl or
  PropertyPage resolves to its module, and its file is the module's. **Read off the form rather than found
  by scanning**: `ModuleDefinition.UpdateFormPart` now records the link on the form too, because a scan
  answers "a plain form" in the several statements between a UserControl's halves being joined and its
  module joining the project — in both the creation path and the load path — and that identity compares
  unequal to every later one.
- [x] 1.2 Both stores, both gutters and the F9 and Ctrl+F2 commands take the identity. `ClearProject` was
  added to both stores for the unload path and **filters the store's own keys**, never the project's current
  documents: a document removed while it carried marks is in neither list, and its entry would be
  unreachable *and* would hold the whole project graph alive through the definition's `Owner`. Found and
  fixed while re-keying: `BookmarkService.SetBookmarks(document, [])` emptied the store without raising its
  change event, so the gutter kept its dots and the sidecar never rewrote — against the `set_bookmarks`
  tool's own description.
- [x] 1.3 All six gone. The live push is scoped through a new `IRunScope`, which holds the running project
  as a **value rather than an event**: the editor that most needs it is the one opened *by* a break, so it
  was never listening when the event fired. It is a service of its own because an editor cannot depend on
  the runner (the cycle runs through the editor factory). `RevealBreak` now opens the editor in the running
  project rather than the startup one.
- [x] 1.4 One converter, `DocumentWireName.For`. All eight sites go through it.
- [x] 1.5 Keyed by the document's name within its project's file. **Old `vb6://` keys are still read**, and
  the two are told apart by the key itself: a VB6 name cannot look like a URI, so re-keying costs no version
  step and `version` still records the line base, which this change does not move. Unload and clear are
  scoped by project through `ClearProject`, and `FindProjectForUri` is gone — the identity carries its
  project. Duplicate names within one project union their lines rather than overwriting, so a legacy project
  holding both a form and a module called `Thing` cannot lose one's marks to the other.
- [x] 1.6 One resolver (`DocumentLookup.Find`) across every loaded project, case-insensitive, with an
  optional `project` argument; an ambiguous bare name is refused and the refusal lists the candidates.
  Replies carry `project`, `document` and the wire `uri`, and a mutating reply reports what the document
  holds afterwards — including when that is nothing. Recorded in `docs/mcp-server-gaps.md`.
  `clear_all_breakpoints`'s description is corrected to say it clears every loaded project; whether VB6
  agrees with that is filed as #492.
- [x] 1.7 Generation (`ProjectNaming.NextFreeName`) is the lowest unused index pooled across **every** kind,
  which the two Add Form commands and the four Add Module commands now share; new project names likewise.
  Adoption refuses a taken or invalid name and says which name it objected to, because Add File is
  multi-select. A form's rename is refused at the root component's Name property; a project's in the
  properties dialog, which now shows the reason beside the box rather than only disabling OK. Five
  `Str.Naming.Msg.*` keys, translated into all 29 shipped packs.
  - **A module, class, UserControl or PropertyPage cannot be renamed through any path in the IDE**:
    `ModuleDefinition.Name` has a public setter with no assignment anywhere outside its constructor, and the
    Properties window binds only a form designer. So the rename half of this task covers forms and projects,
    which are the only two renames that exist. The rule itself is in `ProjectNaming` and applies wherever a
    module rename is eventually built. Filed as #493.
- [x] 1.7b Project-qualified **overloads** rather than trailing optional arguments on the interface methods,
  deviating from this task's letter for a measured reason: an add-in is loaded as a pre-built assembly
  through `Assembly.Load`, and C# bakes a default argument into the *call site*, so a default parameter
  keeps an add-in compiling and breaks every one already packaged with `MissingMethodException`. The records
  do take trailing optional fields, which is the precedent this task cites and where it holds.
  `IProjectAccess.GetProjects()` was added, without which the new argument is undiscoverable. The
  file-opened and file-closed events carry the document, and a tab with no document reports an empty path
  rather than its title.
- [x] 1.8 `DocumentIdentityTests`, `DocumentLookupTests`, `ProjectNamingTests`, `ProjectNameRefusalTests`,
  `CurrentStatementBarTests`, plus the re-keyed `BreakpointServiceTests`, `UserSidecarBreakpointTests` and
  `AddExistingFileTests`. The sidecar's on-disk key is asserted by reading the file, which no round-trip
  through two service instances could show. **Both halves of the current-statement check were proved by
  mutation**: removing the project test fails the two-projects case, and removing the name test fails the
  renamed-module case.

## 2. Names on the wire

> **#489 is settled and does not gate this phase** (2026-09-20). Measured against the real VB6 IDE: no file
> is written for a form or module until the project is saved, in a saved project as much as an unsaved one.
> So a pathless document is a state this phase must *name*, not one the IDE could have designed away. 2.1 is
> unchanged. See the design record, and `docs/vb6-fidelity-oracle.md` for the measurement.

- [x] 2.1 The seam converter: `file:` from the document's own path when it has one (for a UserControl or
  PropertyPage, the module's path); otherwise
  `untitled:<Project>/<Name>.<ext>`, extension from the kind, no leading slash, built through the URI type so
  it is percent-encoded. Fixed when the session opens, never read live from the path.
  — `DocumentWireName.For` is the whole change; `LspDocumentUri.ForUntitled` owns the spelling, beside
  `ForFile`, because construction and comparison have to agree. The extension comes from a new
  `DocumentIdentity.Extension`, read off the kind rather than off a path a pathless document does not have.
  **Fixing the name at session open is 2.5's half** — this answers what a session should be *opened* under.
  Found on the way: `DocumentIdentity.AbsolutePath` was `module?.AbsolutePath ?? form!.AbsolutePath`, which
  reads correctly and throws for any module with no file — `??` evaluates the right side and dereferences a
  form that is null by construction. Nothing had asked a pathless document for its path until this did.
- [x] 2.2 `LspDocumentUri`: `untitled` compares its path without regard to case; remove the `vb6` rules.
  — Both segments are VB6 names. Asserted that an `untitled:` URI survives normalisation at all, on its own,
  because it has no authority and is therefore not the hierarchical shape every `file:` case exercises: a
  scheme the URI parser handled differently would make the other cases pass for the wrong reason.
- [x] 2.3 The reverse: a server's reply naming a document resolves to its identity (the existing file-path
  resolver in `EditorService`, plus an `untitled:` branch). Used by diagnostics, definition, rename and
  workspace-symbol results.
  — No `untitled:` branch was needed in the end: `EditorService.Names` already compared the URI against
  `DocumentWireName.For(document)` as well as the file path, and 2.1 made the first of those the `untitled:`
  spelling. The two branches now answer the same string for a document that has a file and diverge only for
  one that does not, which is what lets a single method answer for modules, forms and carried files alike.
  Proved by rewriting the navigation tests onto the new spellings, plus two cases the retired scheme could
  not express at all: a `Module1` in another project of the same group, and the same name with the wrong
  extension.
- [ ] 2.4 Close and reopen on a name change: on the save event when the path differs from the session's
  (never on the path changing, which a build does temporarily), and on a rename of a document or its project
  when the document has no file. Close then open, ordered per connection, asserted on the wire. Save
  notification afterwards under the new name.
- [ ] 2.4a Two signals the trigger needs and does not have: saving a project into another directory repoints
  every form without raising the save event (add it, beside the module loop that already does), and a form's
  rename is not announced until the designer's pending state is flushed (the "layout changed" notification of
  3.3 carries it).
- [ ] 2.4b Diagnostics under the old name are withdrawn as the close is sent: the pull result id is dropped,
  and for a push server the client records an empty set for that name on that connection, so the ledger and
  the caches keyed on it clear through the existing channel.
- [ ] 2.5 Route every request through the session's current name, gated on the session being open, as the
  carried-file editor already does. The Object Browser's request for a document nobody opened goes through the
  same resolver.
- [x] 2.6 Retire scheme routing: `SchemeLanguageOf`, the scheme branch of `ClaimantsFor` and the identifier as
  a claim.
  — Done **after** 2.7 and 2.8, per 2.7a, and that ordering earned itself: the branch was already
  unreachable, so removing it first would have turned every negative routing assertion vacuous instead of
  red, with nothing left to notice.
  — The identifier's claim is the one behavioural loss, and it is asserted rather than left implicit.
  `AnEntryDeclaringTheSchemeLanguageStillWorksWhateverItsExtensions` is **inverted** into
  `DeclaringTheLanguageWithoutClaimingAnExtensionNoLongerRoutesAnything`, with a companion,
  `DeclaringTheLanguageIsStillWhatLetsAnEntryBeOfferedAProjectsClassModules`, so that the inversion cannot
  be read as "the identifier means nothing now" and the 2.7 gate deleted after it. Nothing real regresses:
  an entry claiming a made-up extension and nothing else was never going to be handed a `.bas` on disk
  either, so this removes an inconsistency rather than a capability.
  — `OnlyHexIdesOwnSchemeNamesALanguage` tested a function that no longer exists and is **replaced** by
  `OnlyAVb6ClaimEstablishesVb6`, which covers what took its job. Its `.md` row is the load-bearing one:
  said loosely as "an extension no other language uses", the rule would admit a Markdown server.
  — The sweep: the `Vb6Doc` / `Form1` / `Form1Uri` constants in five suites, the workspace-symbol open, the
  foreign-server exclusion guard and one integration publish. Two `vb6://` uses are deliberately left —
  `UserSidecarService` still *reads* the old key, which is the migration phase 1 recorded, and
  `ConversationDisclosureTests` uses opaque URIs that 2.10 owns.
- [x] 2.7 The project-member gate: a project member on an ambiguous extension is offered only to servers that
  claim a VB6 extension no other language uses (every VB6 source extension but `.cls`) or declare `vb6`.
  Membership is stated by the caller when the document is opened and remembered with the session — the
  workspace projection exposes only a directory and folders, and neither parsing an `untitled:` name nor
  matching a path can answer it. Change, close and save route by the same record.
  — **Done first, and it turned out to be repairing a live regression rather than adding a guard.**
  `SchemeLanguageOf` requires a literal `://`, which neither `untitled:` nor a `file:` URI's `vb6` test can
  satisfy, so once 2.1 stopped minting `vb6://` the scheme branch of `ClaimantsFor` became unreachable and
  the `.cls` gate went with it. Both existing guards stayed green because both opened
  `vb6://module/Module1` — a string with no extension, so routing returned no claimants and the assertion
  held without reaching the rule it names. Proved by writing the same assertion against
  `untitled:Project1/Class1.cls`: red before this task, green after. `ALatexServerClaimingClsIsNotOfferedVb6Modules`
  was **retargeted rather than supplemented**, because a guard that cannot fail is worse than no guard, and
  it now runs against real texlab.
  — The predicate is `DocumentLanguage.EstablishesVb6(extensions, languageId)`, lifted from the dead
  `Claims`. Applying it to *every* member is the same rule as applying it only on ambiguous extensions, so
  there is one predicate and no second test: on `.bas`, `.frm`, `.ctl` or `.pag` the extension that matched
  is itself unambiguous, so the entry satisfies the gate and it is the identity. `.cls` is the only VB6
  source extension it can exclude, and excluding it is the point.
  — An **overload** on `ILspClient`, never a defaulted `bool`. A `bool` defaulted after the cancellation
  token compiles at every call site and then silently changes what each means: every
  `Received().OpenDocumentAsync(uri, text, Arg.Any<CancellationToken>())` would assert against an implicit
  `false` production no longer passes, and the resulting red reads as a routing regression. The same
  reasoning as the add-in overloads in phase 1, for the same reason.
  — The registry's record is keyed **ordinally**, deliberately not `LspDocumentUri.Comparer`: the
  per-connection tracker it shadows uses the default comparer, and two stores keyed differently
  desynchronise on exactly the case-folding case a URI comparer exists for. `CloseDocumentAsync` routes
  **before** dropping the record, or the close would reach a different set of servers than the open did.
  — The five view-model assertions now pin the **value** (`true` from the code window, `false` from the
  carried-file editor) rather than `Arg.Any`, because stating the wrong one is the defect.
- [ ] 2.7a Order 2.6 after 2.7, and prove the gate before the scheme goes. `Claims()` exists because of
  #277, where a VB6 server attached as `vba` started, initialized and was then never sent a document, and it
  is reachable today only from the branch 2.6 retires. Retiring that branch before 2.7's gate is built and
  asserted against a real foreign server reopens exactly that silence, with nothing to catch it.
- [x] 2.8 Open documents survive a root restart: the registry re-opens every document it knows is open on
  each restarted connection before forwarding any change (#469; required here).
  — Driven from 2.7's record rather than from the triggering document's claimants, because those are two
  different sets and the teardown used the wider one: every entry is stopped, while the caller goes on to
  start and open only the claimants of the one document that triggered it. So a Markdown server holding a
  carried file was stopped with nothing to re-open it.
  — The record carries the latest **text**, updated on change **before** the change is routed. Between the
  teardown and the next server starting, `StartedClaimantsFor` yields nothing and a change goes nowhere;
  remembering only text that had been routed successfully would re-open the document with the text from
  before it and silently undo what was typed.
  — The triggering URI is skipped, because its caller opens it immediately afterwards. It is not yet in the
  record on a first open — the record is written after the restart check returns — but a re-open of a
  document already known would otherwise be sent twice.
  — `RestartIfWorkspaceMovedAsync` now holds a `SemaphoreSlim` across the whole of the teardown and the
  replay. Without it two opens can both pass the `SameDirectory` check before either reaches
  `_rootedAt = current`, which is ordinary — loading a `.vbg` opens several editors and nothing gates
  `LspDocumentSession.Start`. That used to race to a duplicate `StopAsync` and be swallowed; it would now
  replay every open document twice.
  — **The five tests were each checked by removal, not just written.** Deleting the replay reddens two;
  the other three pass vacuously without it, which is recorded here rather than left to be discovered:
  they constrain the replay's behaviour and only bite once it exists. Removing the triggering-URI skip
  reddens the duplicate guard specifically.
  — Not covered, and filed as a gap rather than papered over: **Save Project As of an already-saved project
  moves the root with no document event at all**, because `SaveProject` hardcodes `saveAs: false` for every
  document and only the `.vbp` is repointed. It is a 2.8 trigger with no 2.4 counterpart.
- [ ] 2.9 Compiler diagnostics injected under the wire name resolved from the compiler's own (absolute) file
  path, for every kind, not only forms by file stem. Parse the format the compiler actually writes (0.3), and
  convert its line number there and nowhere else: file line = `N + 1 + hidden lines above it`, where hidden
  means the header **and** every `Attribute` line above the error, so the count is computed from the document
  rather than taken as a constant per kind. Depends on #477.
- [ ] 2.10 Export redaction pseudonymises `untitled:` path segments; the redactor's rationale and the
  disclosure's grouping key are rewritten.
- [ ] 2.11 Tests, one per scenario in this phase's deltas, plus the rewrites the retired scheme forces: the routing tests that open `vb6://`, the ambiguous-extension
  guards against a real foreign server (now asserting the project-member gate, plus a carried `.cls` still
  reaching it), the per-server identifier test, the scheme theory. New: close-before-open asserted on the wire
  bytes, in the style of the shutdown wire-shape tests; a build sends nothing; a first save of a pathless form.
- [ ] 2.12 Acceptance: `demo/spring-tide` carries its module as `Module=` rather than `RelatedDoc=`, and the
  demo's server reads it from disk.

## 3. The whole file in the code window

- [ ] 3.1 Keep a form's, UserControl's and PropertyPage's designer text (`VERSION` through the root `End`) as
  read, on the definition beside the model, so the interpreter, the syntax check and the standalone runner can
  compose the file without a code window. A reload adopts it with the rest of the fidelity state.
- [ ] 3.2 Compose the buffer as prefix plus `Code`, and split there on flush. **The prefix is not the protected
  region**: for `.bas`/`.cls` it is the whole header; for `.frm`/`.ctl`/`.pag` it is the designer part alone,
  because their `Code` already begins with the leading `Attribute` run; where load split nothing off (an
  unparseable `.ctl`/`.pag`, a `.bas`/`.cls` whose header was not recognised — #472) it is empty and the buffer
  is `Code`. Prepending the attribute run to a form would show it twice; splitting after it would strip
  `VB_Name` out of `Code` and write a form without one.
- [ ] 3.2a Dirty detection compares the buffer's body — split exactly as the flush splits it — with `Code`,
  for modules and forms alike. Comparing the whole buffer would class every open document as edited and turn
  every external change into a conflict, disabling the silent reload the file-watcher capability requires.
- [ ] 3.3 A "layout changed" notification on the form, raised on a designer commit and by the three paths that
  bypass the designer's undo stack today: the menu editor, the colour palette, and automation's property set
  with no designer open. It flushes the designer's working collections into the model before rendering (they
  run ahead of it until the apply-unsaved-changes event, so a render before the flush misses the control just
  added), refreshes the prefix once per commit, never per drag step, and carries a rename of the form.
- [ ] 3.3a A save is the second refresh trigger: the prefix becomes the header that was just written, before
  the save is announced, so the buffer follows the file even when the render differs from what was read and
  when a companion reference changes with the file's name. A reload is the third.
- [ ] 3.3b A refresh that changes the prefix's line count shifts that document's breakpoints and bookmarks by
  the difference, in the stores rather than the gutter, so a document with no open code window moves too.
- [ ] 3.4 Header render for a form with no file uses `<Name>.frx`. A form held read-only is never re-rendered.
- [ ] 3.5 Where a document's header carries `VB_Name`, it follows a rename, as an edit the IDE makes itself
  (#473: a form's is never retargeted today, so a renamed form's file names two different forms). A form
  HexIDE created carries no attribute block at all, unlike one imported from VB6, which writes five — a
  fidelity gap of its own, recorded rather than fixed here.
- [ ] 3.6 The interpreter and the pre-run syntax check parse the whole text (after 0.2).
- [ ] 3.7 Protection: one section provider subclassing the stock one over the header and member-attribute
  regions, overriding both of its virtual methods — refusing insertion at a region's edges, which it allows,
  and widening a deletion over a member's attribute run so deleting the line it describes takes the run with
  it. It also carries the whole-document verdict, because binding the editor's read-only property would
  replace the provider outright, and it is re-evaluated on reload. A read-only
  *region* is the header or a member's attribute run and nothing else: a form held read-only as a whole must
  still take breakpoints and answer Find, so the mark, Find and attribute rules test the region, never the
  whole-document gate.
- [ ] 3.8 Undo, by the mechanism 0.5 settled: record the refresh in a marked group so every offset stays
  valid; when the developer undoes and the stack reports that group as the most recent, revert it, undo the
  edit beneath, and re-apply the current header. The redo stack is lost at that point, which is stated in the
  release notes rather than left to be discovered. Test: a code edit, a designer move, then undo — the edit is
  undone, the move is not, and a second undo still undoes the right text.
- [ ] 3.9 One guarded write path, with the policy in the design record for each of the twenty programmatic
  writers: formatting reduced to changed lines and clipped; server rename refused if it touches the header;
  Replace, Replace All, completion, Insert File, Enter auto-close, event stubs, add-in `SetContent` and
  `ApplyEdits`, automation `set_file_content`, `type_text` and `press_key`; reload and the Edit-and-Continue
  revert as owner.
- [ ] 3.10 The bundled server keeps to its own new requirement: no diagnostic inside a header, the formatter
  leaves it untouched, and rename and highlight skip it and member attribute runs, through one shared helper
  so the three cannot drift apart. The client clipping stays as the guard against servers that do not.
- [ ] 3.11 Find and Replace search outside read-only regions only.
- [ ] 3.12 Marks refused on read-only lines, including a gutter click on a folded header.
- [ ] 3.13 Edits the IDE makes itself do not raise Edit-and-Continue's reset prompt.
- [ ] 3.14 Folds: the header fold is merged into every fold application, including an empty or absent server
  answer, with the merged list sorted by start offset (the manager throws otherwise) and zero-length folds
  discarded (it skips them silently). It sets its own folded state rather than relying on the library's
  closed-by-default flag, which applies only on the first update. Folded when created or re-created unless
  expanded in this window. Overlapping server folds dropped.
- [ ] 3.15 Greying: a named palette colour for each theme, meeting the dark palette's recorded contrast bar,
  applied after syntax colouring. Theme packs carry the key.
- [ ] 3.16 Line numbers from the top of the file in the margin, status bar, Call Stack, automation and add-in
  surfaces. Record the divergence from VB6's code-window numbering.
- [ ] 3.17 Sidecar migration to the next format (#466 first, so an older build keeps what it cannot read): read the recorded format, re-key, move lines by each header's
  length, carry unmeasurable entries unchanged, keep unrecognised content, rewrite only on change and never
  after a failed read.
- [ ] 3.18 Automation: `get_file_content` returns the whole file open or not; `set_file_content` accepts it
  back, or a body alone with the header kept. Tool descriptions say lines count from the top of the file.
- [ ] 3.19 Add-ins: content is the whole file; positions from the top of the file; `SetContent` refused if the
  header changes. The AI Chat add-in's prompt and apply paths updated.
- [ ] 3.20 New strings (fold labels, refusal reasons, the reworded read-only banner) added to `en` and every
  shipped pack in the same change.
- [ ] 3.21 Tests, one per scenario in the deltas this phase implements, named after the scenario, plus: buffer equals the file on open; an unchanged save writes the buffer; formatting leaves the
  header; Replace All does not touch a control's `Begin` line; a designer move is not undone by code-window
  undo; the header stays folded after formatting; a migrated sidecar keeps marks on their statements and is
  idempotent; an add-in replacement that changes the header is refused.

- [ ] 3.22 Verify in the running IDE, through the automation tools rather than by asking anyone to click:
  open a VB6-authored `.frm`, `.cls` and `.bas`; snapshot the header folded and greyed under a light and a
  dark theme; confirm typing, Replace All and F9 in the header are refused and say why; confirm the margin
  and status bar count from the top of the file; confirm a designer move updates the header with the code
  window open. UI work is not complete until it is seen running.

## 4. Member attributes

- [ ] 4.1 Detect procedure-level and declaration-level `Attribute` runs, and anchor each to the line it
  describes.
- [ ] 4.2 Read-only and greyed, through the same provider and colour as the header.
- [ ] 4.3 Folds from the end of the described line, built with character offsets and nested inside a server's
  procedure fold.
- [ ] 4.4 A member's rename rewrites its own attribute qualifiers — a procedure, property or module-level
  variable alike, since VB6 writes `VB_Var*` lines after a declaration. A server rename's edits to them are
  permitted; its edits to the header are not.
- [ ] 4.5 Enter at the end of a described line inserts after the attribute run, not between the line and its
  attributes.
- [ ] 4.5a A run follows the line it describes: deleting that line deletes the run with it (the provider's
  deletable span widens over the run), and a cut takes it along. An orphaned run, however it arises, is
  preserved as inert text.
- [ ] 4.6 Corpus lane: files carrying member-level attributes round-trip, which no lane covers today.
- [ ] 4.7 Tests, one per scenario in this phase's delta, plus a deletion case for a described procedure.
- [ ] 4.8 Verify in the running IDE, through the automation tools: a procedure's attributes folded into its
  declaration line, refusing an edit, and surviving a format.

## 5. Documentation

- [ ] 5.1 `docs/lsp-client.md`: identifiers, the scheme retirement, the project-member gate, close and reopen.
- [ ] 5.2 `docs/language-servers.md`: extensions are the only claim; `languageId` names a language and routes
  nothing except as the ambiguity gate.
- [ ] 5.3 `docs/lsp-server-features.md`: the example trace line; client-made folds; the formatter's header
  rule; what the server now accepts.
- [ ] 5.4 `docs/mcp-server-gaps.md`: every automation contract change, recorded and filed.
- [ ] 5.5 `docs/debugger-vb6-divergences.md`: the visible header, file-counted line numbers, and designer
  changes exempt from the reset prompt.
- [ ] 5.6 `docs/MISSING_FEATURES.md` and `docs/MISSING_LANGUAGE.md`: line numbers, folding, rename, Find and
  the Procedure Attributes row (#62). The attribute rows whose mechanism text says "stripped from the body".
- [ ] 5.7 Comments that describe the old model: `FormCodeText`, `ModuleFileFormat`, `ModuleDefinition`,
  `DirtyDetector`, the redactor and disclosure, `DocumentSavedEvent`, `IBreakpointService`, the automation
  tool descriptions.
- [ ] 5.8 Validate and archive; rewrite the Purpose of each spec the archive touched if the CLI replaces it,
  and fill in the new capabilities' Purpose.
